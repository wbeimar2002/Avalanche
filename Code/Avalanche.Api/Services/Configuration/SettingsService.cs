using Avalanche.Api.Services.Files;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Security.Grpc.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{ 
    public class SettingsService : ISettingsService
    {
        readonly IConfigurationService _configurationService;
        readonly IStorageService _storageService;

        readonly string _hostIpAddress;

        public Ism.Streaming.V1.Protos.WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        public bool IgnorePgsTimeoutClientMocks { get; set; }

        public SettingsService(IConfigurationService configurationService, IStorageService storageService)
        {
            _configurationService = configurationService;
            _storageService = storageService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var PgsTimeoutGrpcPort = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            WebRtcStreamerClient = ClientHelper.GetInsecureClient<Ism.Streaming.V1.Protos.WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{WebRTCGrpcPort}");
            PgsTimeoutClient = ClientHelper.GetInsecureClient<PgsTimeout.PgsTimeoutClient>($"https://{_hostIpAddress}:{PgsTimeoutGrpcPort}");
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            //Faking calls while I have the real server
            if (!IgnorePgsTimeoutClientMocks)
            {
                var mockResponseForPdf = new GetTimeoutPdfPathResponse()
                {
                    PdfPath = @"C:\Olympus\apps\config\AvalancheApi\safety_checklist.pdf"
                };

                var mockResponseForAlwaysOn = new GetPgsAlwaysOnSettingResponse()
                {
                    IsAlwaysOn = true
                };

                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();

                var fakeCallForPdf = TestCalls.AsyncUnaryCall(Task.FromResult(mockResponseForPdf), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });

                var fakeCallForAlwaysOn = TestCalls.AsyncUnaryCall(Task.FromResult(mockResponseForAlwaysOn), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });

                mockGrpcClient.Setup(mock => mock.GetTimeoutPdfPathAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCallForPdf);
                mockGrpcClient.Setup(mock => mock.GetPgsAlwaysOnSettingAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCallForAlwaysOn);

                PgsTimeoutClient = mockGrpcClient.Object;
            }
          
            var actionResponseForPdf = await PgsTimeoutClient.GetTimeoutPdfPathAsync(new Empty());
            var actionResponseForAlwaysOn = await PgsTimeoutClient.GetPgsAlwaysOnSettingAsync(new Empty());

            return new TimeoutSettings
            {
                CheckListFileName = actionResponseForPdf.PdfPath,
                PgsVideoAlwaysOn = actionResponseForAlwaysOn.IsAlwaysOn
            };
        }

        public async Task<SetupSettings> GetSetupSettingsAsync()
        {
            return await _storageService.GetJson<SetupSettings>("PatientsSetupSettings", 1);
        }

        public async Task<VideoRoutingSettings> GetVideoRoutingSettingsAsync()
        {
            return await _storageService.GetJson<VideoRoutingSettings>("VideoRoutingSettings", 1);
        }
    }
}
