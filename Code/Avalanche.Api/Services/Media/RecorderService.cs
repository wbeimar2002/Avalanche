using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Recorder.Common.Core;
using Ism.Security.Grpc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class RecorderService : IRecorderService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public Recorder.RecorderClient RecorderClient { get; set; }

        public RecorderService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            RecorderClient = ClientHelper.GetInsecureClient<Recorder.RecorderClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        public async Task StartRecording()
        {
            await RecorderClient.StartRecordingAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task StopRecording()
        {
            await RecorderClient.StopRecordingAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
