using System.Threading.Tasks;
using Ism.Routing.V1.Protos;

namespace Avalanche.Api.Services.Media
{
    public interface IRoutingService
    {
        Task<GetVideoSourcesResponse> GetVideoSources();
        Task<GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(GetAlternativeVideoSourceRequest request);
        Task<GetVideoSinksResponse> GetVideoSinks();

        Task<GetCurrentRoutesResponse> GetCurrentRoutes();
        Task<GetVideoStateForSourceResponse> GetVideoStateForSource(GetVideoStateForSourceRequest getVideoStateForSourceRequest);
        Task<GetRouteForSinkResponse> GetRouteForSink(GetRouteForSinkRequest getRouteForSinkRequest);
        Task EnterFullScreen(EnterFullScreenRequest enterFullScreenRequest);
        Task ExitFullScreen(ExitFullScreenRequest exitFullScreenRequest);
        Task RouteVideo(RouteVideoRequest routeVideoRequest);
        Task RouteVideoBatch(RouteVideoBatchRequest routeVideoBatchRequest);
        Task<GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources();
        Task<GetTileLayoutsForSinkResponse> GetLayoutsForSink(GetTileLayoutsForSinkRequest sink);
    }
}
