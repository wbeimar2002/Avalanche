using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class RoutingService : IRoutingService
    {
        readonly IConfigurationService _configurationService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly string _hostIpAddress;

        public Ism.Routing.V1.Protos.Routing.RoutingClient RoutingClient { get; set; }

        public RoutingService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            RoutingClient = ClientHelper.GetInsecureClient<Ism.Routing.V1.Protos.Routing.RoutingClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoSourcesResponse> GetVideoSources()
        {
            return await RoutingClient.GetVideoSourcesAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
        public async Task<Ism.Routing.V1.Protos.GetVideoSinksResponse> GetVideoSinks()
        {
            return await RoutingClient.GetVideoSinksAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetCurrentRoutes()
        {
            return await RoutingClient.GetCurrentRoutesAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoStateForSourceResponse> GetVideoStateForSource(Ism.Routing.V1.Protos.GetVideoStateForSourceRequest getVideoStateForSourceRequest)
        {
            return await RoutingClient.GetVideoStateForSourceAsync(getVideoStateForSourceRequest);
        }

        public async Task EnterFullScreen(Ism.Routing.V1.Protos.EnterFullScreenRequest enterFullScreenRequest)
        {
            await RoutingClient.EnterFullScreenAsync(enterFullScreenRequest);
        }

        public async Task ExitFullScreen(Ism.Routing.V1.Protos.ExitFullScreenRequest exitFullScreenRequest)
        {
            await RoutingClient.ExitFullScreenAsync(exitFullScreenRequest);
        }

        public async Task<Ism.Routing.V1.Protos.GetRouteForSinkResponse> GetRouteForSink(Ism.Routing.V1.Protos.GetRouteForSinkRequest getRouteForSinkRequest)
        {
            return await RoutingClient.GetRouteForSinkAsync(getRouteForSinkRequest);
        }

        public async Task RouteVideo(Ism.Routing.V1.Protos.RouteVideoRequest routeVideoRequest)
        {
            await RoutingClient.RouteVideoAsync(routeVideoRequest);
        }
    }
}
