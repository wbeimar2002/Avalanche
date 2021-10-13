using Ism.Routing.Client.V1;
using Ism.Routing.V1.Protos;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RoutingService : IRoutingService
    {
        private readonly RoutingSecureClient _client;
        public RoutingService(RoutingSecureClient client) => _client = client;

        public async Task EnterFullScreen(EnterFullScreenRequest enterFullScreenRequest) =>
            await _client.EnterFullScreen(enterFullScreenRequest).ConfigureAwait(false);

        public async Task ExitFullScreen(ExitFullScreenRequest exitFullScreenRequest) =>
            await _client.ExitFullScreen(exitFullScreenRequest).ConfigureAwait(false);

        public async Task<GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(GetAlternativeVideoSourceRequest request) =>
            await _client.GetAlternativeVideoSource(request).ConfigureAwait(false);

        public async Task<GetCurrentRoutesResponse> GetCurrentRoutes() =>
            await _client.GetCurrentRoutes().ConfigureAwait(false);

        public async Task<GetRouteForSinkResponse> GetRouteForSink(GetRouteForSinkRequest getRouteForSinkRequest) =>
            await _client.GetRouteForSink(getRouteForSinkRequest).ConfigureAwait(false);

        public async Task<GetVideoSinksResponse> GetVideoSinks() =>
            await _client.GetVideoSinks().ConfigureAwait(false);

        public async Task<GetVideoSourcesResponse> GetVideoSources() =>
            await _client.GetVideoSources().ConfigureAwait(false);

        public async Task<GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources() =>
            await _client.GetVideoStateForAllSources().ConfigureAwait(false);

        public async Task<GetVideoStateForSourceResponse> GetVideoStateForSource(GetVideoStateForSourceRequest getVideoStateForSourceRequest) =>
            await _client.GetVideoStateForSource(getVideoStateForSourceRequest).ConfigureAwait(false);

        public async Task RouteVideo(RouteVideoRequest routeVideoRequest) =>
            await _client.RouteVideo(routeVideoRequest).ConfigureAwait(false);

        public async Task RouteVideoBatch(RouteVideoBatchRequest routeVideoBatchRequest) =>
            await _client.RouteVideoBatch(routeVideoBatchRequest).ConfigureAwait(false);

        public async Task<GetTileLayoutsForSinkResponse> GetLayoutsForSink(GetTileLayoutsForSinkRequest sink) =>
            await _client.GetTileLayoutsForSink(sink).ConfigureAwait(false);

        public async Task<GetTileLayoutResponse> GetLayoutForSink(GetTileLayoutRequest sink) =>
            await _client.GetTileLayoutForSink(sink).ConfigureAwait(false);

        public async Task SetLayoutForSink(SetTileLayoutRequest sink) =>
            await _client.SetTileLayoutForSink(sink).ConfigureAwait(false);
    }
}
