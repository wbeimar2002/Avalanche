using AutoMapper;

using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;

using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;

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

        // gRPC client for the pgs timeout application
        private readonly IPgsTimeoutService _pgsTimeoutService;

        // mapper for various gRPC types to api types
        private readonly IMapper _mapper;

        // pgs manager
        private readonly IPgsManager _pgsManager;


        // cancellation token for the start/stop lock
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Lock used for starting/stopping pgs/timeout
        /// </summary>
        private readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        private PgsTimeoutModes _priorPgsTimeoutState = PgsTimeoutModes.Idle;

        private IList<RouteModel> _prePgsRoutes;

        private GetCurrentRoutesResponse _currentRoutes = new GetCurrentRoutesResponse();


        public TimeoutManager(
            IStorageService storageService,
            IRoutingService routingService,
            IPgsTimeoutService pgsTimeoutService,
            IPgsManager pgsManager,
            IMapper mapper)
        {
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _pgsTimeoutService = ThrowIfNullOrReturn(nameof(pgsTimeoutService), pgsTimeoutService);
            _pgsManager = ThrowIfNullOrReturn(nameof(pgsManager), pgsManager);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
        }

        #region Routing and State Orchestation
        public async Task StartTimeout()
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                var state = await GetPgsTimeoutPlayerState();
                if (state == PgsTimeoutModes.Timeout)
                {
                    // Timeout already started
                    return;
                }

                // If PGS is actively playing save the last state as pgs
                if ((await _pgsTimeoutService.GetPgsPlaybackState()).IsPlaying)
                {
                    _priorPgsTimeoutState = PgsTimeoutModes.Pgs;
                }

                // Save previous routes
                if (_priorPgsTimeoutState == PgsTimeoutModes.Pgs)
                {
                    await SavePrePgsRoutes();
                }

                await SaveCurrentRoutes();

                // Ask PGS player to set timeout mode
                await _pgsTimeoutService.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout });

                // Route timeout
                var config = await _storageService.GetJsonObject<TimeoutConfiguration>(nameof(TimeoutConfiguration), 1, ConfigurationContext.FromEnvironment());

                var sinks = await GetTimeoutSinks();

                var routes = sinks.Select(x => new RouteVideoRequest
                {
                    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(config.Source),
                    Sink = _mapper.Map<VideoDeviceModel, AliasIndexMessage>(x)
                });

                var request = new RouteVideoBatchRequest();
                request.Routes.AddRange(routes);
                await _routingService.RouteVideoBatch(request);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task StopTimeout(bool restoreLastRoutes)
        {
            await _startStopLock.WaitAsync(_cts.Token);
            try
            {
                PgsTimeoutModes state = await GetPgsTimeoutPlayerState();
                if (state == PgsTimeoutModes.Timeout)
                {
                    var playerMode = new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeIdle };
                    if (restoreLastRoutes && _priorPgsTimeoutState == PgsTimeoutModes.Pgs)
                    {
                        playerMode.Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs;
                    }

                    // Ask PGS player to stop timeout mode
                    await _pgsTimeoutService.SetPgsTimeoutMode(playerMode);

                    if (restoreLastRoutes) // Restore last saved routes
                    {
                        await LoadCurrentSavedRoutes();
                    }

                    await _pgsTimeoutService.SetPgsMute(new SetPgsMuteRequest()
                    {
                        IsMuted = true
                    });
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        private async Task<PgsTimeoutModes> GetPgsTimeoutPlayerState()
        {
            return (PgsTimeoutModes)(await _pgsTimeoutService.GetPgsTimeoutMode()).Mode;
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
            return result == null ? -1 : result.PageNumber;
        }

        public async Task<int> GetTimeoutPageCount()
        {
            var result = await _pgsTimeoutService.GetTimeoutPageCount();
            return result == null ? -1 : result.PageCount;
        }

        public async Task<string> GetTimeoutPdfPath()
        {
            var result = await _pgsTimeoutService.GetTimeoutPdfPath();
            var fileName = result?.PdfPath;

            var timeoutRoot = Environment.GetEnvironmentVariable("TimeoutDataRoot");
            var relative = Path.Combine(timeoutRoot, fileName);

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

        public async Task DeActivateTimeout()
        {
            // If we are currently in timeout state then timeout is not stopped
            // Stop timeout and restore pre pgs routes
            var state = await GetPgsTimeoutPlayerState();
            if (state == PgsTimeoutModes.Timeout)
            {
                await StopTimeout(false);

                if (_priorPgsTimeoutState == PgsTimeoutModes.Pgs)
                {
                    await LoadPrePgsRoutes();
                }
                else
                {
                    await LoadCurrentSavedRoutes();
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task<IList<VideoSinkModel>> GetTimeoutSinks()
        {
            // This needs to return the same data that routing does
            var timeoutSinksData = await _storageService.GetJsonObject<SinksData>("TimeoutSinksData", 1, ConfigurationContext.FromEnvironment());

            var routingSinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            // Timeout sinks are a subset of routing sinks
            // Get the routing sinks that are also called out in the Timeout sink collection
            var timeoutSinks = routingSinks.VideoSinks
                .Where(routingSink =>
                    timeoutSinksData.Items
                    .Any(timeoutSink =>
                        string.Equals(timeoutSink.Alias, routingSink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && timeoutSink.Index == routingSink.Sink.Index));

            var apiSinks = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSinkModel>>(timeoutSinks.ToList());

            foreach (var sink in apiSinks)
            {
                var route = routes.Routes.SingleOrDefault(x =>
                    string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && x.Sink.Index == sink.Sink.Index);

                // get the current source
                sink.Source = new AliasIndexModel()
                {
                    Alias = route.Source.Alias,
                    Index = route.Source.Index
                };
            }
            return apiSinks;
        }

        private async Task LoadCurrentSavedRoutes()
        {
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(_currentRoutes.Routes.Select(x => new RouteVideoRequest { Sink = x.Sink, Source = x.Source }));
            await _routingService.RouteVideoBatch(request);
        }

        private async Task LoadPrePgsRoutes()
        {
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(_prePgsRoutes.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x.Sink)
            }));

            if (request.Routes.Any())
                await _routingService.RouteVideoBatch(request);
        }

        private async Task SaveCurrentRoutes()
        {
            // TODO: 4ko tile routes are not supported currently
            // don't need to worry about those until RX4
            var state = await GetPgsTimeoutPlayerState();
            if (state != PgsTimeoutModes.Idle)
                return;

            _currentRoutes = await _routingService.GetCurrentRoutes();
        }

        private async Task SavePrePgsRoutes()
        {
            _prePgsRoutes = await _pgsManager.GetPrePgsRoutes();
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
