using Avalanche.Shared.Infrastructure.Services.Settings;
using AvidisDeviceInterface.Client.V1;
using AvidisDeviceInterface.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static AvidisDeviceInterface.V1.Protos.Avidis;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class AvidisService : IAvidisService
    {
        readonly IConfigurationService _configurationService;
        AvidisSecureClient AvidisClient { get; set; }

        public AvidisService(IConfigurationService configurationService, IGrpcClientFactory<AvidisClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            AvidisClient = new AvidisSecureClient(grpcClientFactory, hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        public async Task RoutePreview(AvidisDeviceInterface.V1.Protos.RoutePreviewRequest routePreviewRequest)
        {
            await AvidisClient.RoutePreview(routePreviewRequest);
        }

        public async Task ShowPreview(ShowPreviewRequest showPreviewRequest)
        {
            await AvidisClient.ShowPreview(showPreviewRequest);
        }

        public async Task HidePreview(HidePreviewRequest hidePreviewRequest)
        {
            await AvidisClient.HidePreview(hidePreviewRequest);
        }
    }
}
