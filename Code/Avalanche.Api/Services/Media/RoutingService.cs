using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Routing.Common.Core;
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

        public bool IgnoreGrpcServicesMocks { get; set; }

        public Routing.RoutingClient RoutingClient { get; set; }

        public RoutingService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            RoutingClient = ClientHelper.GetInsecureClient<Routing.RoutingClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        public async Task<GetVideoSourcesResponse> GetVideoSources()
        {
            return await RoutingClient.GetVideoSourcesAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
        public async Task<GetVideoSinksResponse> GetVideoSinks()
        {
            return await RoutingClient.GetVideoSinksAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task<GetCurrentRoutesResponse> GetCurrentRoutes()
        {
            return await RoutingClient.GetCurrentRoutesAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task<GetVideoStateForSourceResponse> GetVideoStateForSource(GetVideoStateForSourceRequest getVideoStateForSourceRequest)
        {
            return await RoutingClient.GetVideoStateForSourceAsync(getVideoStateForSourceRequest);
        }

        public async Task EnterFullScreen(EnterFullScreenRequest enterFullScreenRequest)
        {
            await RoutingClient.EnterFullScreenAsync(enterFullScreenRequest);
        }

        public async Task ExitFullScreen(ExitFullScreenRequest exitFullScreenRequest)
        {
            await RoutingClient.ExitFullScreenAsync(exitFullScreenRequest);
        }
    }
}
