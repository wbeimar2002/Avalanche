using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IRoutingManager
    {
        Task RoutePreview(AvidisDeviceInterface.V1.Protos.RoutePreviewRequest routePreviewRequest);
        Task ShowPreview(AvidisDeviceInterface.V1.Protos.ShowPreviewRequest showPreviewRequest);
        Task HidePreview(AvidisDeviceInterface.V1.Protos.HidePreviewRequest hidePreviewRequest);

        Task<Ism.Routing.V1.Protos.GetVideoSourcesResponse> GetVideoSources();
        Task<Ism.Routing.V1.Protos.GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest request);
        Task<Ism.Routing.V1.Protos.GetVideoSinksResponse> GetVideoSinks();
        Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetCurrentRoutes();
        Task<Ism.Routing.V1.Protos.GetVideoStateForSourceResponse> GetVideoStateForSource(Ism.Routing.V1.Protos.GetVideoStateForSourceRequest getVideoStateForSourceRequest);
        Task<Ism.Routing.V1.Protos.GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources();
        Task EnterFullScreen(Ism.Routing.V1.Protos.EnterFullScreenRequest enterFullScreenRequest);
        Task ExitFullScreen(Ism.Routing.V1.Protos.ExitFullScreenRequest exitFullScreenRequest);
        Task<Ism.Routing.V1.Protos.GetRouteForSinkResponse> GetRouteForSink(Ism.Routing.V1.Protos.GetRouteForSinkRequest getRouteForSinkRequest);
        Task RouteVideo(Ism.Routing.V1.Protos.RouteVideoRequest routeVideoRequest);
    }
}
