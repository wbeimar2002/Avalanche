using AutoMapper;
using Avalanche.Api.Extensions;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.Client.V1;
using Ism.Routing.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using Ism.SystemState.Client;
using Ism.SystemState.Models.PgsTimeout;
using Ism.SystemState.Models.VideoRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.PgsTimeout
{
    /// <summary>
    /// Handles routing pgs/timeout
    /// saving and restoring the current routes
    /// controlling the pgs timeout player
    /// setting the enabled state for pgs displays
    /// </summary>
    public class PgsTimeoutManager : IPgsTimeoutManager, IDisposable
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


        //cancellation token for the sarat/stop lock
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Lock used for starting/stopping pgs/timeout
        /// </summary>
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        // fake values for pgs volume and mute
#warning add endpoints to pgs player for this

        private readonly PgsVideoFile _generalFile = new PgsVideoFile { Name = "General", FilePath = @"C:\Resources\PgsVideo\General.mp4" };
        private readonly PgsVideoFile _pediatricFile = new PgsVideoFile { Name = "Pediatric", FilePath = @"C:\Resources\PgsVideo\Pediatric.mp4" };

        private double _pgsVolume = 0.5;
        private bool _pgsMute = false;
        private PgsVideoFile _currentVideoFile;

        public PgsTimeoutManager(
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

            _currentVideoFile = _generalFile;
        }

        public async Task StartPgs()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                await SaveCurrentRoutes();



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
                await LoadSavedRoutes();

                // TODO: might need to revisit state tracking when we need to implement timeout
                _currentPgsTimeoutState = PgsTimeoutModes.Idle;
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

            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task StopTimeout()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {

            }
            finally
            {
                _startStopLock.Release();
            }
        }

        #region PgsTimeoutPlayer methods

        public async Task<IList<PgsVideoFile>> GetPgsVideoFiles()
        {
            // TODO: integrate with player app
            //var files = await _pgsTimeoutService.GetPgsVideoFileList();
            var files = new List<PgsVideoFile> { _generalFile, _pediatricFile };
            return await Task.FromResult(files);
        }

        public async Task<PgsVideoFile> GetPgsVideoFile() 
        {
            // TODO:
            return await Task.FromResult(_currentVideoFile);
        }

        public async Task SetPgsVideoFile(PgsVideoFile file)
        {
            // TODO: integrate with player
            //await _pgsTimeoutService.SetPgsVideoFile(new Ism.PgsTimeout.V1.Protos.SetPgsVideoFileRequest { VideoFile = path });
            _currentVideoFile = file;
            await Task.CompletedTask;
        }

        public async Task SetPlaybackPosition(double position)
        {
            // TODO: implement set video position
            await Task.CompletedTask;
        }

        public async Task<double> GetPgsVolume()
        {
            return await Task.FromResult(_pgsVolume);
        }

        public async Task SetPgsVolume(double volume)
        {
            _pgsVolume = volume;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Returns true if PGS is muted
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetPgsMute()
        {
            return await Task.FromResult(_pgsMute);
        }

        /// <summary>
        /// Set to true to mute PGS audio
        /// </summary>
        /// <param name="mute"></param>
        /// <returns></returns>
        public async Task SetPgsMute(bool mute)
        {
            _pgsMute = mute;
            await Task.CompletedTask;
        }

        #endregion

        #region API/routing methods

        public async Task<IList<VideoSink>> GetPgsSinks()
        {
            // this needs to return the same data that routing does
            var config = await GetConfig();

            var routingSinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            // PGS sinks are typically a subset of routing sinks
            // typically, they would be all of the displays without the record channels

            // get the routing sinks that are also called out in the pgs sink collection
            var pgsSinks = routingSinks.VideoSinks.Where(routingSink =>
                config.PgsSinks.Any(pgsSink =>
                    routingSink.Sink.EqualsOther(pgsSink.ToRoutingAliasIndex())));

            var apiSinks = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSink>>(pgsSinks.ToList());

            foreach (var sink in apiSinks)
            {
                var route = routes.Routes.SingleOrDefault(x => x.Sink.EqualsOther(sink));
                // get the current source
                sink.Source = new AliasIndexApiModel(route.Source.Alias, route.Source.Index);
            }

            return apiSinks;
        }

        public Task<IList<VideoSink>> GetTimeoutSinks()
        {
            throw new NotImplementedException();
        }

        public async Task SetPgsStateForSink(AliasIndexApiModel displayId, bool enabled)
        {
            var config = await GetConfig();
            // pgs checkbox state must persist reboots
            // state client should handle this
            // if pgs is activated, video route for that display needs to be restored
            // start pgs-> uncheck display A -> A gets its route restored

            // pgs is active, restore save/restore pgs for this display
            if (_currentPgsTimeoutState == PgsTimeoutModes.Pgs)
            {
                // get the existing route
                var route = _currentRoutes.Routes.SingleOrDefault(x =>
                        string.Equals(x.Sink.Alias, displayId.Alias, StringComparison.OrdinalIgnoreCase) &&
                        x.Sink.Index == displayId.Index);

                if (enabled)
                {
                    // send pgs back to this display
                    await _routingService.RouteVideo(new RouteVideoRequest
                    {
                        Source = config.PgsSource.ToRoutingAliasIndex(),
                        Sink = displayId.ToRoutingAliasIndex()
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
                            Sink = displayId.ToRoutingAliasIndex()
                        });
                    }
                }
            }

            // update the state data
            await _stateClient.UpdateData<PgsDisplayStateData>(x =>
            {
                // default state won't have an entry for a display
                var existing = x.DisplayStates.SingleOrDefault(x =>
                    string.Equals(x.AliasIndex.Alias, displayId.Alias, StringComparison.OrdinalIgnoreCase) &&
                    x.AliasIndex.Index == displayId.Index);

                // add entry or update existing
                if (existing == null)
                    x.DisplayStates.Add(new PgsDisplayState { AliasIndex = new AliasIndexModel(displayId.Alias, displayId.Index), Enabled = enabled });
                else
                    existing.Enabled = enabled;
            });
        }

        public async Task<bool> GetPgsStateForSink(AliasIndexApiModel displayId)
        {
            // pgs checkbox state must persist reboots
            // state client should handle this
            var pgsData = await _stateClient.GetData<PgsDisplayStateData>();
            var state = pgsData.DisplayStates.SingleOrDefault(x =>
                string.Equals(x.AliasIndex.Alias, displayId.Alias, StringComparison.OrdinalIgnoreCase) &&
                x.AliasIndex.Index == displayId.Index);

            return state?.Enabled ?? true;
        }

        #endregion

        private async Task<PgsTimeoutConfig> GetConfig()
        {
            return await _storageService.GetJsonObject<PgsTimeoutConfig>(nameof(PgsTimeoutConfig), 1, ConfigurationContext.FromEnvironment());
        }


        private GetCurrentRoutesResponse _currentRoutes = new GetCurrentRoutesResponse();

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

            foreach (var route in _currentRoutes.Routes)
            {
                await _routingService.RouteVideo(new RouteVideoRequest { Sink = route.Sink, Source = route.Source });
            }
        }


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
