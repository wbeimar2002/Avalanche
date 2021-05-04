using Ism.Routing.Client.V1;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RoutingService : IRoutingService
    {
        private readonly RoutingSecureClient _client;
        public RoutingService(RoutingSecureClient client)
        {
            _client = client;
        }

        public async Task EnterFullScreen(Ism.Routing.V1.Protos.EnterFullScreenRequest enterFullScreenRequest)
        {
            await _client.EnterFullScreen(enterFullScreenRequest);
        }

        public async Task ExitFullScreen(Ism.Routing.V1.Protos.ExitFullScreenRequest exitFullScreenRequest)
        {
            await _client.ExitFullScreen(exitFullScreenRequest);
        }

        public async Task<Ism.Routing.V1.Protos.GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest request)
        {
            return await _client.GetAlternativeVideoSource(request);
        }

        public async Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetCurrentRoutes()
        {
            return await _client.GetCurrentRoutes();
        }

        public async Task<Ism.Routing.V1.Protos.GetRouteForSinkResponse> GetRouteForSink(Ism.Routing.V1.Protos.GetRouteForSinkRequest getRouteForSinkRequest)
        {
            return await _client.GetRouteForSink(getRouteForSinkRequest);
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoSinksResponse> GetVideoSinks()
        {
            return await _client.GetVideoSinks();
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoSourcesResponse> GetVideoSources()
        {
            return await _client.GetVideoSources();
        }
        public async Task<Ism.Routing.V1.Protos.GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources()
        {
            return await _client.GetVideoStateForAllSources();
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoStateForSourceResponse> GetVideoStateForSource(Ism.Routing.V1.Protos.GetVideoStateForSourceRequest getVideoStateForSourceRequest)
        {
            return await _client.GetVideoStateForSource(getVideoStateForSourceRequest);
        }

        public async Task RouteVideo(Ism.Routing.V1.Protos.RouteVideoRequest routeVideoRequest)
        {
            await _client.RouteVideo(routeVideoRequest);
        }

        public async Task RouteVideoBatch(Ism.Routing.V1.Protos.RouteVideoBatchRequest routeVideoBatchRequest)
        {
            await _client.RouteVideoBatch(routeVideoBatchRequest);
        }
    }
}
