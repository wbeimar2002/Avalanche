using Avalanche.Shared.Infrastructure.Services.Settings;
using AvidisDeviceInterface.Proto;
using Ism.Security.Grpc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class AvidisService : IAvidisService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public Avidis.AvidisClient AvidisClient { get; set; }

        public AvidisService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            AvidisClient = ClientHelper.GetInsecureClient<Avidis.AvidisClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        public async Task RoutePreview(RoutePreviewRequest routePreviewRequest)
        {
            await AvidisClient.RoutePreviewAsync(routePreviewRequest);
        }

        public async Task SetPreviewRegion(SetPreviewRegionRequest setPreviewRegionRequest)
        {
            await AvidisClient.SetPreviewRegionAsync(setPreviewRegionRequest);
        }

        public async Task SetPreviewVisible(SetPreviewVisibleRequest setPreviewVisibleRequest)
        {
            await AvidisClient.SetPreviewVisibleAsync(setPreviewVisibleRequest);
        }
    }
}
