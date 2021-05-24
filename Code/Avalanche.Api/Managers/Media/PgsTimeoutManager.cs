using AutoMapper;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.PgsTimeout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Media
{
    /// <summary>
    /// Handles routing pgs/timeout
    /// saving and restoring the current routes
    /// controlling the pgs timeout player
    /// setting the enabled state for pgs displays
    /// </summary>
    public class PgsTimeoutManager : IPgsTimeoutManager, IDisposable
    {

        // used internally to route video and store current routes
        private readonly IRoutingService _routingService;

        // publish pgs display state and room state
        private readonly IStateClient _stateClient;

        // gRPC client for the pgs timeout application
        private readonly IPgsTimeoutService _pgsTimeoutService;

        // mapper for various gRPC types to api types
        private readonly IMapper _mapper;

        private readonly PgsApiConfiguration _pgsConfig;
        private readonly TimeoutApiConfiguration _timeoutConfig;

        /// <summary>
        /// Video routes from befoe PGS or timeout started
        /// </summary>
        private GetCurrentRoutesResponse _currentRoutes = new GetCurrentRoutesResponse();

        /// <summary>
        /// The current room state. Note that this is different than the pgs player application state
        /// </summary>
        private PgsTimeoutRoomState _currentPgsTimeoutState = PgsTimeoutRoomState.Idle;

        /// <summary>
        /// Sets the room mode and publishes the state event
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private async Task SetRoomMode(PgsTimeoutRoomState mode)
        {
            _currentPgsTimeoutState = mode;
            await _stateClient.PublishEvent(new PgsTimeoutRoomStateEvent { RoomState = mode });
        }

        // used to handle the pgs->timeout state edge cases
        //private PgsTimeoutModes _previousPgsTimeoutState = PgsTimeoutModes.Idle;

        // cancellation token for the start/stop lock
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Lock used for starting/stopping pgs/timeout
        /// </summary>
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        public PgsTimeoutManager(
            IRoutingService routingService,
            IStateClient stateClient,
            IPgsTimeoutService pgsTimeoutService,
            IMapper mapper,
            PgsApiConfiguration pgsConfig,
            TimeoutApiConfiguration timeoutConfig)
        {
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _stateClient = ThrowIfNullOrReturn(nameof(stateClient), stateClient);
            _pgsTimeoutService = ThrowIfNullOrReturn(nameof(pgsTimeoutService), pgsTimeoutService);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _pgsConfig = ThrowIfNullOrReturn(nameof(pgsConfig), pgsConfig);
            _timeoutConfig = ThrowIfNullOrReturn(nameof(timeoutConfig), timeoutConfig);
        }

        public Task<PgsTimeoutRoomState> GetRoomState() => Task.FromResult(_currentPgsTimeoutState);

        public async Task<IList<PgsSinkStateModel>> GetPgsStateForSinks()
        {
            // get current pgs state data
            var pgsData = await _stateClient.GetData<PgsDisplayStateData>();

            // generate a list of pgs sink state models
            var displayStates = _pgsConfig.Sinks.Select(x =>
                new PgsSinkStateModel
                {
                    Sink = x,
                    // if the state data has no entry for a display, it defaults to checked
                    Enabled = pgsData?.DisplayStates.SingleOrDefault(y => AliasIndexEquals((y.AliasIndex.Alias, y.AliasIndex.Index), (x.Alias, x.Index)))?.Enabled ?? true
                }).ToList();

            return displayStates;
        }

        public async Task<IList<VideoSinkModel>> GetPgsSinks()
        {
            // this needs to return the same data that routing does
            // the displays ui component is used here
            var routingSinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            // PGS sinks are typically a subset of routing sinks
            // typically, they would be all of the displays without the record channels

            // get the routing sinks that are also called out in the pgs sink collection
            var pgsSinks = routingSinks.VideoSinks.Where(routingSink =>
                _pgsConfig.Sinks.Any(
                    pgsSink => AliasIndexEquals((pgsSink.Alias, pgsSink.Index), (routingSink.Sink.Alias, routingSink.Sink.Index))));

            var apiSinks = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSinkModel>>(pgsSinks.ToList());

            foreach (var sink in apiSinks)
            {
                var route = routes.Routes.SingleOrDefault(x => AliasIndexEquals((x.Sink.Alias, x.Sink.Index), (sink.Sink.Alias, sink.Sink.Index)));

                // get the current source
                // this should give the UI enough info to show the icon on the display
                sink.Source = new AliasIndexModel()
                {
                    Alias = route.Source.Alias,
                    Index = route.Source.Index
                };
            }

            return apiSinks;
        }

        public async Task SetPgsStateForSink(PgsSinkStateViewModel sinkState)
        {
            bool enabled = sinkState.Enabled;
            // pgs checkbox state must persist reboots
            // state client should handle this
            // if pgs is activated, video route for that display needs to be restored
            // start pgs-> uncheck display A -> A gets its route restored

            // ensure this is a valid pgs sink
            var sinkExists = _pgsConfig.Sinks.Any(x => AliasIndexEquals((x.Alias, x.Index), (sinkState.Sink.Alias, sinkState.Sink.Index)));

            if (!sinkExists)
                throw new InvalidOperationException($"No pgs sink exists for: {sinkState.Sink.Alias}:{sinkState.Sink.Index}");

            // pgs is active, restore save/restore pgs for this display
            if (_currentPgsTimeoutState == PgsTimeoutRoomState.Pgs)
                await UpdatePgsOnOneSink(sinkState.Sink, enabled, _pgsConfig.Source);

            var currentData = await _stateClient.GetData<PgsDisplayStateData>();
            var displayIndex = currentData?.DisplayStates.FindIndex(x => AliasIndexEquals((x.AliasIndex.Alias, x.AliasIndex.Index), (sinkState.Sink.Alias, sinkState.Sink.Index))) ?? -1;

            // need to prepare the initial state
            var newPgsData = new PgsDisplayStateData
            {
                DisplayStates = new List<PgsDisplayState>
                {
                    new PgsDisplayState
                    {
                        AliasIndex = new Ism.SystemState.Models.VideoRouting.AliasIndexModel(sinkState.Sink.Alias, sinkState.Sink.Index),
                        Enabled = sinkState.Enabled
                    }
                }
            };

            // update the state data
            await _stateClient.AddOrUpdateData(newPgsData, x =>
            {
                // default state won't have an entry for a display
                if (displayIndex < 0)
                {
                    x.Add(data => data.DisplayStates, new PgsDisplayState { AliasIndex = new Ism.SystemState.Models.VideoRouting.AliasIndexModel(sinkState.Sink.Alias, sinkState.Sink.Index), Enabled = enabled });
                }
                else
                {
                    x.Replace(data => data.DisplayStates[displayIndex].Enabled, enabled);
                }
            });
        }

        public async Task<TimeoutModes> GetTimeoutMode()
        {
            return await Task.FromResult(_timeoutConfig.Mode);
        }

        #region PgsTimeout player pass through

        /// <summary>
        /// Sets the pgs/timeout mode of the player application
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task SetPgsTimeoutPlayerMode(PgsTimeoutModes mode)
        {
            await _pgsTimeoutService.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest()
            {
                Mode = (PgsTimeoutModeEnum)mode
            });
        }

        public async Task<PgsTimeoutModes> GetPgsTimeoutPlayerMode()
        {
            var res = await _pgsTimeoutService.GetPgsTimeoutMode();
            return (PgsTimeoutModes)res.Mode;
        }

        // pgs methods
        public async Task<bool> GetPgsMute()
        {
            var result = await _pgsTimeoutService.GetPgsMute();
            return result.IsMuted;
        }

        public async Task SetPgsVideoPlaybackState(bool isPlaying)
        {
            await _pgsTimeoutService.SetPgsPlaybackState(new SetPgsPlaybackStateRequest()
            {
                IsPlaying = isPlaying
            });
        }

        public async Task<bool> GetPgsVideoPlaybackState()
        {
            var result = await _pgsTimeoutService.GetPgsPlaybackState();
            return result.IsPlaying;
        }

        public async Task SetPgsVideoFile(GreetingVideoModel video)
        {
            var request = _mapper.Map<GreetingVideoModel, SetPgsVideoFileRequest>(video);
            await _pgsTimeoutService.SetPgsVideoFile(request);
        }

        public async Task<GreetingVideoModel> GetPgsVideoFile()
        {
            var result = await _pgsTimeoutService.GetPgsVideoFile();
            return _mapper.Map<GetPgsVideoFileResponse, GreetingVideoModel>(result);
        }

        public async Task<List<GreetingVideoModel>> GetPgsVideoFileList()
        {
            var result = await _pgsTimeoutService.GetPgsVideoFileList();
            return _mapper.Map<IList<PgsVideoFileMessage>, IList<GreetingVideoModel>>(result.VideoFiles).ToList();
        }

        public async Task<double> GetPgsVolume()
        {
            var result = await _pgsTimeoutService.GetPgsVolume();
            return result.Volume;
        }

        public async Task SetPgsMute(bool isMuted)
        {
            await _pgsTimeoutService.SetPgsMute(new SetPgsMuteRequest()
            {
                IsMuted = isMuted
            });
        }

        public async Task SetPgsVideoPosition(double position)
        {
            await _pgsTimeoutService.SetPgsVideoPosition(new SetPgsVideoPositionRequest()
            {
                Position = position
            });
        }

        public async Task SetPgsVolume(double volume)
        {
            await _pgsTimeoutService.SetPgsVolume(new SetPgsVolumeRequest()
            {
                Volume = volume
            });
        }

        // timeout methods
        public async Task SetTimeoutPage(int pageNumber)
        {
            await _pgsTimeoutService.SetTimeoutPage(new SetTimeoutPageRequest()
            {
                PageNumber = pageNumber
            });
        }

        public async Task<int> GetTimeoutPage()
        {
            var result = await _pgsTimeoutService.GetTimeoutPage();
            return result.PageNumber;
        }

        public async Task<int> GetTimeoutPageCount()
        {
            var result = await _pgsTimeoutService.GetTimeoutPageCount();
            return result.PageCount;
        }

        public async Task<string> GetTimeoutPdfFileName()
        {
            // returns something like "timeout.pdf"
            var result = await _pgsTimeoutService.GetTimeoutPdfFileName();

            // return a path to the mapped directory
            var timeoutRoot = Environment.GetEnvironmentVariable("TimeoutDataRoot");
            var relative = Path.Combine(timeoutRoot, result.FileName);

            var translated = relative.Replace('\\', '/').TrimStart('/');

            return "/" + translated;
        }

        public async Task NextPage()
        {
            await _pgsTimeoutService.NextPage();
        }

        public async Task PreviousPage()
        {
            await _pgsTimeoutService.PreviousPage();
        }

        #endregion

        #region Start/Stop

        /// <summary>
        /// Saves the current routes
        /// Sends pgs to all checked displays
        /// Put the player in pgs mode
        /// Tell the player to play if it was paused
        /// </summary>
        /// <returns></returns>
        public async Task StartPgs()
        {
            // pgs can be started from idle, or indirectly when stopping timeout
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                // save what is currently routed to all of the displays
                // does nothing if the mode is not idle
                await SaveCurrentRoutes();

                // get the collection of pgs displays and their checked state
                var displays = await GetPgsStateForSinks();

                var request = new RouteVideoBatchRequest();
                // create route message for all enabled displays
                foreach (var sink in displays)
                {
                    // display is unchecked, skip this one
                    if (!sink.Enabled)
                        continue;

                    request.Routes.Add(new RouteVideoRequest
                    {
                        Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(_pgsConfig.Source),
                        Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sink.Sink)
                    });
                }

                // route pgs to all enabled displays
                // it is possible to uncheck all of the displays before starting pgs
                // we don't want to batch route "nothing"
                if (request.Routes.Any())
                    await _routingService.RouteVideoBatch(request);

                // tell the player to go to pgs mode
                // note that the player's idle mode is different than api's idle mode
                await SetPgsTimeoutPlayerMode(PgsTimeoutModes.Pgs);

                // tell the player to play video if it isn't
                await SetPgsVideoPlaybackState(true);

                // unmute the player
                await SetPgsMute(false);

                // set the room mode to pgs which ends up publishing the event
                await SetRoomMode(PgsTimeoutRoomState.Pgs);
            }
            finally
            {
                _startStopLock.Release();
            }
        }


        public async Task StartTimeout()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                // this won't save routes if the current mode is pgs or timeout
                await SaveCurrentRoutes();

                // tell the player to activate timeout mode
                // TODO: this behavior will need to be revisited when we implement external video source timeout
                await SetPgsTimeoutPlayerMode(PgsTimeoutModes.Timeout);

                // go to the first page when starting timeout
                await SetTimeoutPage(0);

                // route timeout to all displays
                var request = new RouteVideoBatchRequest();
                foreach (var sink in _timeoutConfig.Sinks)
                {
                    request.Routes.Add(new RouteVideoRequest
                    {
                        Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(_timeoutConfig.Source),
                        Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sink)
                    });
                }

                if (request.Routes.Any())
                    await _routingService.RouteVideoBatch(request);

                if (_timeoutConfig.Mode == TimeoutModes.VideoSource)
                {
                    // TODO: implement properly when we need to
                    //await _routingService.EnterFullScreen(new EnterFullScreenRequest
                    //{
                    //    UserInterfaceId = 0,
                    //    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(_timeoutConfig.Source)
                    //});
                }

                // room is now in timeout mode
                await SetRoomMode(PgsTimeoutRoomState.Timeout);
            }
            finally
            {
                _startStopLock.Release();
            }
        }


        /// <summary>
        /// Call this when stopping pgs or timeout
        /// </summary>
        /// <returns></returns>
        public async Task StopPgsAndTimeout()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                // no pgs or timeout, do nothing
                if (_currentPgsTimeoutState == PgsTimeoutRoomState.Idle)
                    return;

                // tell the player to go back to looping the video
                await SetPgsTimeoutPlayerMode(PgsTimeoutModes.Pgs);

                // ensure the video is playing
                await SetPgsVideoPlaybackState(true);

                // mute the audio as this stops pgs
                await SetPgsMute(true);

                // load the pre pgs/timeout routes
                await LoadSavedRoutes();

                // room mode is now idle
                await SetRoomMode(PgsTimeoutRoomState.Idle);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Saves the current video routes only if the room is not in pgs or timeout mode
        /// </summary>
        /// <returns></returns>
        private async Task SaveCurrentRoutes()
        {
            // TODO: determine if previous routes have to survive a reboot
            // for now, they don't
            // TODO: 4ko tile routes are not supported currently
            // don't need to worry about those until RX4

            // only save routes if we're not in pgs/timeout
            if (_currentPgsTimeoutState != PgsTimeoutRoomState.Idle)
                return;

            _currentRoutes = await _routingService.GetCurrentRoutes();
        }

        private async Task LoadSavedRoutes()
        {
            if (_currentPgsTimeoutState == PgsTimeoutRoomState.Idle)
                return;

            // only restore displays that are pgs or timeout displays
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(_currentRoutes.Routes.Where(x =>
                _pgsConfig.Sinks.Any(y => AliasIndexEquals((x.Sink.Alias, x.Sink.Index), (y.Alias, y.Index))) ||
                _timeoutConfig.Sinks.Any(y => AliasIndexEquals((x.Sink.Alias, x.Sink.Index), (y.Alias, y.Index))))
                .Select(x => new RouteVideoRequest { Sink = x.Sink, Source = x.Source }));
            await _routingService.RouteVideoBatch(request);
        }

        /// <summary>
        /// Invoked when checking/unchecking a pgs checkbox whilc pgs mode is active
        /// </summary>
        /// <param name="sink"></param>
        /// <param name="pgsEnabled"></param>
        /// <param name="pgsSource"></param>
        /// <returns></returns>
        private async Task UpdatePgsOnOneSink(AliasIndexViewModel sink, bool pgsEnabled, AliasIndexModel pgsSource)
        {
            // get the saved route for this display
            var route = _currentRoutes.Routes.SingleOrDefault(x => AliasIndexEquals((x.Sink.Alias, x.Sink.Index), (sink.Alias, sink.Index)));

            if (pgsEnabled)
            {
                // send pgs back to this display
                await _routingService.RouteVideo(new RouteVideoRequest
                {
                    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsSource),
                    Sink = _mapper.Map<AliasIndexViewModel, AliasIndexMessage>(sink)
                });
            }
            else
            {
                // restore whatever was routed to this display
                if (route != null)
                {
                    await _routingService.RouteVideo(new RouteVideoRequest
                    {
                        Source = route.Source,
                        Sink = _mapper.Map<AliasIndexViewModel, AliasIndexMessage>(sink)
                    });
                }
            }
        }

        /// <summary>
        /// Restores saved routes on all non pgs enabled displays
        /// </summary>
        /// <returns></returns>
        private async Task RestoreRoutesOnNonPgsDisplays()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                // helps when going from timeout -> pgs
                var pgsSinkStates = await GetPgsStateForSinks();

                // all saved routes minus pgs enabled displays
                // turn into a batch route request
                var routesToRestore = _currentRoutes.Routes.Where(x =>
                    !pgsSinkStates.Any(y =>
                        y.Enabled &&
                        AliasIndexEquals((x.Sink.Alias, x.Sink.Index), (y.Sink.Alias, y.Sink.Index))));

                var request = new RouteVideoBatchRequest();
                request.Routes.AddRange(routesToRestore.Select(x => new RouteVideoRequest { Source = x.Source, Sink = x.Sink }));
                await _routingService.RouteVideoBatch(request);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        /// <summary>
        /// Helper for all those aliasindex equals comparisons
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool AliasIndexEquals((string Alias, string Index) left, (string Alias, string Index) right)
        {
            return string.Equals(left.Alias, right.Alias, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(left.Index, right.Index, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _startStopLock.Dispose();
                _disposed = true;
            }
        }

        ~PgsTimeoutManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
