using Avalanche.Api.Services.Media;
using AvidisDeviceInterface.V1.Protos;
using Ism.Routing.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class RoutingManager : IRoutingManager
    {
        readonly IRoutingService _routingService;
        readonly IAvidisService _avidisService;

        public RoutingManager(IRoutingService routingService, IAvidisService avidisService)
        {
            _routingService = routingService;
            _avidisService = avidisService;
        }

        public async Task EnterFullScreen(EnterFullScreenRequest enterFullScreenRequest)
        {
            await _routingService.EnterFullScreen(enterFullScreenRequest);
        }

        public async Task ExitFullScreen(ExitFullScreenRequest exitFullScreenRequest)
        {
            await _routingService.ExitFullScreen(exitFullScreenRequest);
        }

        public async Task<GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(GetAlternativeVideoSourceRequest request)
        {
            return await _routingService.GetAlternativeVideoSource(request);
        }

        public async Task<GetCurrentRoutesResponse> GetCurrentRoutes()
        {
            return await _routingService.GetCurrentRoutes();
        }

        public async Task<GetRouteForSinkResponse> GetRouteForSink(GetRouteForSinkRequest getRouteForSinkRequest)
        {
            return await _routingService.GetRouteForSink(getRouteForSinkRequest);
        }

        public async Task<GetVideoSinksResponse> GetVideoSinks()
        {
            return await _routingService.GetVideoSinks();
        }

        public async Task<GetVideoSourcesResponse> GetVideoSources()
        {
            return await _routingService.GetVideoSources();
        }

        public async Task<GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources()
        {
            return await _routingService.GetVideoStateForAllSources();
        }

        public async Task<GetVideoStateForSourceResponse> GetVideoStateForSource(GetVideoStateForSourceRequest getVideoStateForSourceRequest)
        {
            return await _routingService.GetVideoStateForSource(getVideoStateForSourceRequest);
        }

        public async Task RouteVideo(RouteVideoRequest routeVideoRequest)
        {
            await _routingService.RouteVideo(routeVideoRequest);
        }

        public async Task HidePreview(HidePreviewRequest hidePreviewRequest)
        {
            await _avidisService.HidePreview(hidePreviewRequest);
        }

        public async Task RoutePreview(RoutePreviewRequest routePreviewRequest)
        {
            await _avidisService.RoutePreview(routePreviewRequest);
        }

        public async Task ShowPreview(ShowPreviewRequest showPreviewRequest)
        {
            await _avidisService.ShowPreview(showPreviewRequest);
        }
    }
}
