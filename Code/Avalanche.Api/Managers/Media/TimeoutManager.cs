using AutoMapper;

using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Models.Configuration;

using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Media
{
    /// <summary>
    /// Handles routing timeout
    /// saving and restoring the current routes
    /// controlling the pgs timeout player
    /// setting the enabled state for timeout displays
    /// </summary>
    public class TimeoutManager : ITimeoutManager, IDisposable
    {
        // used to get pgs configuration
        private readonly IStorageService _storageService;

        // used internally to route video and store current routes
        private readonly IRoutingService _routingService;

        // used for persisting and publishing the checkbox state for the timeout displays
        private readonly IStateClient _stateClient;

        // gRPC client for the pgs timeout application
        private readonly IPgsTimeoutService _pgsTimeoutService;

        // mapper for various gRPC types to api types
        private readonly IMapper _mapper;

        // pgs manager
        private readonly IPgsManager _pgsManager;


        //cancellation token for the sarat/stop lock
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Lock used for starting/stopping pgs/timeout
        /// </summary>
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 
        /// </summary>
        private PgsTimeoutModes _currentPgsTimeoutState = PgsTimeoutModes.Idle;

        private PgsTimeoutModes _lastPgsTimeoutState = PgsTimeoutModes.Idle;

        private GetCurrentRoutesResponse _prePgsRoutes = new GetCurrentRoutesResponse();
        private GetCurrentRoutesResponse _currentRoutes = new GetCurrentRoutesResponse();


        public TimeoutManager(
            IStorageService storageService,
            IRoutingService routingService,
            IStateClient stateClient,
            IPgsTimeoutService pgsTimeoutService,
            IPgsManager pgsManager,
            IMapper mapper)
        {
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _stateClient = ThrowIfNullOrReturn(nameof(stateClient), stateClient);
            _pgsTimeoutService = ThrowIfNullOrReturn(nameof(pgsTimeoutService), pgsTimeoutService);
            _pgsManager = ThrowIfNullOrReturn(nameof(pgsManager), pgsManager);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
        }

        #region Routing and State Orchestation

        private async Task<IList<VideoSinkModel>> GetTimeoutSinks()
        {
            // this needs to return the same data that routing does
            var config = await GetConfig();

            var routingSinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            // Timeout sinks are typically a subset of routing sinks
            // get the routing sinks that are also called out in the timeout sink collection

            var timeoutSinks = routingSinks.VideoSinks.Where(routingSink =>
                config.TimeoutSinks.Any(timeoutSink => string.Equals(timeoutSink.Alias, routingSink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                && timeoutSink.Index == routingSink.Sink.Index));

            var apiSinks = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSinkMessage>, IList<VideoSinkModel>>(timeoutSinks.ToList());

            foreach (var sink in apiSinks)
            {
                var route = routes.Routes.SingleOrDefault(x => string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                && x.Sink.Index == sink.Sink.Index);

                // get the current source
                sink.Source = new SinkModel()
                {
                    Alias = route.Source.Alias,
                    Index = route.Source.Index
                };
            }

            return apiSinks;
        }

        private async Task StartTimeout()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                if (_currentPgsTimeoutState == PgsTimeoutModes.Timeout)
                {
                    // Timeout already started
                    return;
                }

                // If PGS is actively playing save the last state as pgs
                if ((await _pgsTimeoutService.GetPgsPlaybackState()).IsPlaying)
                {
                    _lastPgsTimeoutState = PgsTimeoutModes.Pgs;
                }

                // Save previous routes
                if(_lastPgsTimeoutState == PgsTimeoutModes.Pgs)
                {
                    await SavePrePgsRoutes();
                }
                await SaveCurrentRoutes();


                // Ask PGS player to set timeout mode
                await _pgsTimeoutService.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout });

                // Route timeout
                var config = await GetConfig();
                var sinks = await GetTimeoutSinks();
                foreach (var sink in sinks)
                {
                    await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest
                    {
                        Source = _mapper.Map<SinkModel, Ism.Routing.V1.Protos.AliasIndexMessage>(config.TimeoutSource),
                        Sink = _mapper.Map<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>(sink)
                    });
                }

                // TODO Audio

                _currentPgsTimeoutState = PgsTimeoutModes.Timeout;
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        private async Task StopTimeout(bool restoreLastRoutes)
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {                
                if (_currentPgsTimeoutState == PgsTimeoutModes.Timeout)
                {
                    var playerMode = new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeIdle};
                    if(restoreLastRoutes && _lastPgsTimeoutState == PgsTimeoutModes.Pgs)
                    {
                        playerMode.Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs;
                    }

                    // Ask PGS player to stop timeout mode
                    await _pgsTimeoutService.SetPgsTimeoutMode(playerMode);

                    if(restoreLastRoutes) // Restore last saved routes
                    {
                        await LoadCurrentSavedRoutes();
                    }

                    // TODO: audio?
                }

                _currentPgsTimeoutState = PgsTimeoutModes.Idle;
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        private async Task LoadPrePgsRoutes()
        {
            if (_currentPgsTimeoutState == PgsTimeoutModes.Idle)
                return;

            foreach (var route in _prePgsRoutes.Routes)
            {
                await _routingService.RouteVideo(new RouteVideoRequest
                {
                    Sink = route.Sink,
                    Source = route.Source
                });
            }
        }

        private async Task LoadCurrentSavedRoutes()
        {
            if (_currentPgsTimeoutState == PgsTimeoutModes.Idle)
                return;

            foreach (var route in _currentRoutes.Routes)
            {
                await _routingService.RouteVideo(new RouteVideoRequest
                {
                    Sink = route.Sink,
                    Source = route.Source
                });
            }
        }

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

        private async Task SavePrePgsRoutes()
        {
            _prePgsRoutes = await _pgsManager.GetPrePgsRoutes();
        }


        #endregion

        #region Timeout

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

        public async Task<string> GetTimeoutPdfPath()
        {
            var result = await _pgsTimeoutService.GetTimeoutPdfPath();
            return result.PdfPath;
        }

        public async Task NextPage()
        {
            await _pgsTimeoutService.NextPage();
        }

        public async Task PreviousPage()
        {
            await _pgsTimeoutService.PreviousPage();
        }

        public async Task SetTimeoutState(bool state)
        {
            // start or stop timeout based on the requested state
            // the timeoutManager deals with pgs-timeout interaction
            // it also deals with something like 2 UIs starting timeout at the same time
            if (state)
                await StartTimeout();
            else
                await StopTimeout(true); 
        }

        public async Task DeActivateTimeout()
        {
            // If we are currently in timeout state then timeout is not stopped
            // Stop timeout and restore pre pgs routes
            if (_currentPgsTimeoutState == PgsTimeoutModes.Timeout)
            {
                await StopTimeout(false);

                if(_lastPgsTimeoutState == PgsTimeoutModes.Pgs)
                {
                    await LoadPrePgsRoutes();
                }
                else
                {
                    await LoadCurrentSavedRoutes ();
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task<PgsTimeoutConfig> GetConfig()
        {
            return await _storageService.GetJsonObject<PgsTimeoutConfig>(nameof(PgsTimeoutConfig), 1, ConfigurationContext.FromEnvironment());
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

        ~TimeoutManager()
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
