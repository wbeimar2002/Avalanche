using AutoMapper;

using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Models;

using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.PgsTimeout;

using System;
using System.Collections.Generic;
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
    public class PgsManager : IPgsManager, IDisposable
    {
        // used to get pgs configuration
        private readonly IStorageService _storageService;

        // used internally to route video and store current routes
        private readonly IRoutingService _routingService;

        // used for persisting and publishing the checkbox state for the pgs displays
        private readonly IStateClient _stateClient;

        // gRPC client for the pgs timeout application
        private readonly IPgsTimeoutService _pgsTimeoutService;

        // mapper for various gRPC types to api types
        private readonly IMapper _mapper;

        /// <summary>
        /// Video routes from befoe PGS or timeout started
        /// </summary>
        private GetCurrentRoutesResponse _currentRoutes = new GetCurrentRoutesResponse();

        /// <summary>
        /// Is the room in PGS, timeout or none
        /// Used to determine if we need to save the current routes or not
        /// 
        /// For example, when pgs is started, we want to save the current routes
        /// If you then go directly to timeout, we don't want to save the current routes because it would just be "PGS on all displays"
        /// We only save routes when transitioning from idle to pgs/timeout
        /// Also, saved routes are only restored when transisioning back to idle
        /// 
        /// Cases to test
        /// idle->pgs->idle
        /// idle->timeout->idle
        /// idle->pgs->timeout->pgs (click finish timeout)
        /// idle->pgs->timeout->idle (start timeout, then navigate to video routing tab)
        /// </summary>
        private PgsTimeoutModes _currentPgsTimeoutState = PgsTimeoutModes.Idle;

        // used to handle the pgs->timeout state edge cases
        private PgsTimeoutModes _previousPgsTimeoutState = PgsTimeoutModes.Idle;


        // cancellation token for the start/stop lock
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Lock used for starting/stopping pgs/timeout
        /// </summary>
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        public PgsManager(
            IStorageService storageService,
            IRoutingService routingService,
            IStateClient stateClient,
            IPgsTimeoutService pgsTimeoutService,
            IMapper mapper)
        {
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _stateClient = ThrowIfNullOrReturn(nameof(stateClient), stateClient);
            _pgsTimeoutService = ThrowIfNullOrReturn(nameof(pgsTimeoutService), pgsTimeoutService);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
        }

        #region Routing and State Orchestation

        public async Task<bool> GetPgsStateForSink(AliasIndexModel sink)
        {
            // pgs checkbox state must persist reboots
            // state client handles this

            // TODO: make this a batch operation
            // front end calls this when the page is loaded
            // should resemble a dictionary<AliasIndex, bool>

            var pgsData = await _stateClient.GetData<PgsDisplayStateData>();

            var state = pgsData?.DisplayStates.SingleOrDefault(x =>
                string.Equals(x.AliasIndex.Alias, sink.Alias, StringComparison.OrdinalIgnoreCase)
                && x.AliasIndex.Index == sink.Index);

            return (state?.Enabled ?? true);
        }


        public async Task<IList<VideoSinkModel>> GetPgsSinks()
        {
            // this needs to return the same data that routing does
            var pgsSinksData = await _storageService.GetJsonObject<List<AliasIndexModel>>("PgsSinks", 1, ConfigurationContext.FromEnvironment());

            var routingSinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            // PGS sinks are typically a subset of routing sinks
            // typically, they would be all of the displays without the record channels

            // get the routing sinks that are also called out in the pgs sink collection

            var pgsSinks = routingSinks.VideoSinks.Where(routingSink =>
                pgsSinksData.Any(pgsSink => string.Equals(routingSink.Sink.Alias, routingSink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                && pgsSink.Index == routingSink.Sink.Index));

            var apiSinks = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSinkModel>>(pgsSinks.ToList());

            foreach (var sink in apiSinks)
            {
                var route = routes.Routes.SingleOrDefault(x => string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                && x.Sink.Index == sink.Sink.Index);

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
            var config = await _storageService.GetJsonObject<PgsConfiguration>(nameof(PgsConfiguration), 1, ConfigurationContext.FromEnvironment());

            // pgs checkbox state must persist reboots
            // state client should handle this
            // if pgs is activated, video route for that display needs to be restored
            // start pgs-> uncheck display A -> A gets its route restored

            // pgs is active, restore save/restore pgs for this display
            if (_currentPgsTimeoutState == PgsTimeoutModes.Pgs)
                await UpdatePgsOnOneSink(sinkState.Sink, enabled, config.Source);

            var currentData = await _stateClient.GetData<PgsDisplayStateData>();
            var displayIndex = currentData?.DisplayStates.FindIndex(x =>
                string.Equals(x.AliasIndex.Alias, sinkState.Sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.AliasIndex.Index, sinkState.Sink.Index, StringComparison.OrdinalIgnoreCase)) ?? -1;

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

        #endregion

        #region PGS Basic Actions

        public async Task<TimeoutModes> GetPgsTimeoutMode()
        {
            var result = await _pgsTimeoutService.GetPgsTimeoutMode();
            return (TimeoutModes)((int)result.Mode);
        }

        public async Task SetPgsTimeoutMode(PgsTimeoutModes mode)
        {
            await _pgsTimeoutService.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest()
            {
                Mode = (PgsTimeoutModeEnum)((int)mode)
            });
        }

        public async Task<bool> GetPgsMute()
        {
            var result = await _pgsTimeoutService.GetPgsMute();
            return result.IsMuted;
        }

        public async Task SetPgsPlaybackState(bool isPlaying)
        {
            await _pgsTimeoutService.SetPgsPlaybackState(new SetPgsPlaybackStateRequest()
            {
                IsPlaying = isPlaying
            });
        }

        public async Task<bool> GetPgsPlaybackState()
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

        public async Task<IList<RouteModel>> GetPrePgsRoutes()
        {
            var routes = _currentRoutes.Routes.Select(x => new RouteModel
            {
                Source = _mapper.Map<AliasIndexMessage, AliasIndexModel>(x.Source),
                Sink = _mapper.Map<AliasIndexMessage, AliasIndexModel>(x.Sink)
            }).ToList();
            return await Task.FromResult(routes);
        }


        public async Task StartPgs()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                var config = await _storageService.GetJsonObject<PgsConfiguration>(nameof(PgsConfiguration), 1, ConfigurationContext.FromEnvironment());

                await SaveCurrentRoutes();

                var sinks = await GetPgsSinks();

                var request = new RouteVideoBatchRequest();
                // create route message for all enabled displays
                foreach (var sink in sinks)
                {
                    // display is unchecked, skip this one
                    var enabled = await GetPgsStateForSink(sink.Sink);
                    if (!enabled)
                        continue;

                    request.Routes.Add(new RouteVideoRequest
                    {
                        Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(config.Source),
                        Sink = _mapper.Map<VideoDeviceModel, AliasIndexMessage>(sink)
                    });
                }

                // route pgs to all enabled displays
                // it is possible to uncheck all of the displays before starting pgs
                // we don't want to batch route "nothing"
                if (request.Routes.Any())
                    await _routingService.RouteVideoBatch(request);

                // tell the player to go to pgs mode
                // note that the player's idle mode is different than api's idle mode
                await _pgsTimeoutService.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs });
                // tell the player to play video if it isn't
                await _pgsTimeoutService.SetPgsPlaybackState(new SetPgsPlaybackStateRequest { IsPlaying = true });
                _currentPgsTimeoutState = PgsTimeoutModes.Pgs;
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task StopPgs()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                // restore saved routes
                await LoadSavedRoutes();

                //Bug: 11910 Solved
                await _pgsTimeoutService.SetPgsMute(new SetPgsMuteRequest()
                {
                    IsMuted = true
                });

                // TODO: might need to revisit state tracking when we need to implement timeout
                _currentPgsTimeoutState = PgsTimeoutModes.Idle;

                await this.SetPgsMute(true);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        #endregion

        #region Private Methods

        private async Task SaveCurrentRoutes()
        {
            // TODO: determine if previous routes have to survive a reboot
            // for now, they don't
            // TODO: 4ko tile routes are not supported currently
            // don't need to worry about those until RX4

            if (_currentPgsTimeoutState != PgsTimeoutModes.Idle)
                return;

            _currentRoutes = await _routingService.GetCurrentRoutes();
        }

        private async Task LoadSavedRoutes()
        {
            if (_currentPgsTimeoutState == PgsTimeoutModes.Idle)
                return;

            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(_currentRoutes.Routes.Select(x => new RouteVideoRequest { Sink = x.Sink, Source = x.Source }));
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
            var route = _currentRoutes.Routes.SingleOrDefault(x =>
                    string.Equals(x.Sink.Alias, sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Sink.Index, sink.Index, StringComparison.OrdinalIgnoreCase));

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

        ~PgsManager()
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
