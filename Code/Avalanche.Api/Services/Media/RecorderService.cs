using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Recorder.Client.V1;
using Ism.Recorder.Core.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;
using static Ism.Recorder.Core.V1.Protos.Recorder;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RecorderService : IRecorderService
    {
        readonly IConfigurationService _configurationService;

        public RecorderSecureClient RecorderClient { get; set; }

        public RecorderService(IConfigurationService configurationService, IGrpcClientFactory<RecorderClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = ThrowIfNullOrReturn(nameof(configurationService), configurationService);
            ThrowIfNull(nameof(grpcClientFactory), grpcClientFactory);
            ThrowIfNull(nameof(certificateProvider), certificateProvider);

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            RecorderClient = new RecorderSecureClient(grpcClientFactory, hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        public async Task StartRecording(RecordMessage recordMessage) => await RecorderClient.StartRecording(recordMessage);

        public async Task StopRecording() => await RecorderClient.StopRecording();

        public async Task CaptureImage(CaptureImageRequest imageMessage) => await RecorderClient.CaptureImage(imageMessage);
    }
}
