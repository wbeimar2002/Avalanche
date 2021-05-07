using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IRoutingService
    {
        Task<Ism.Routing.V1.Protos.GetVideoSourcesResponse> GetVideoSources();
        Task<Ism.Routing.V1.Protos.GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest request);
        Task<Ism.Routing.V1.Protos.GetVideoSinksResponse> GetVideoSinks();


        Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetCurrentRoutes();
        Task<Ism.Routing.V1.Protos.GetVideoStateForSourceResponse> GetVideoStateForSource(Ism.Routing.V1.Protos.GetVideoStateForSourceRequest getVideoStateForSourceRequest);
        Task<Ism.Routing.V1.Protos.GetRouteForSinkResponse> GetRouteForSink(Ism.Routing.V1.Protos.GetRouteForSinkRequest getRouteForSinkRequest);
        Task EnterFullScreen(Ism.Routing.V1.Protos.EnterFullScreenRequest enterFullScreenRequest);
        Task ExitFullScreen(Ism.Routing.V1.Protos.ExitFullScreenRequest exitFullScreenRequest);
        Task RouteVideo(Ism.Routing.V1.Protos.RouteVideoRequest routeVideoRequest);
        Task RouteVideoBatch(Ism.Routing.V1.Protos.RouteVideoBatchRequest routeVideoBatchRequest);
        Task<Ism.Routing.V1.Protos.GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources();
    }
}
