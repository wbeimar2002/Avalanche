using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
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
        readonly string _hostIpAddress;

        public WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        public SettingsService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var PgsTimeoutGrpcPort = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            WebRtcStreamerClient = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{WebRTCGrpcPort}");
            PgsTimeoutClient = ClientHelper.GetInsecureClient<PgsTimeout.PgsTimeoutClient>($"https://{_hostIpAddress}:{PgsTimeoutGrpcPort}");
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            //Faking calls while I have the real server
            var mockResponse = new GetTimeoutPdfPathResponse()
            {
                PdfPath = @"C:\Olympus\apps\config\AvalancheApi\safety_checklist.pdf"
            };

            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(mockResponse), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.GetTimeoutPdfPathAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.GetTimeoutPdfPathAsync(new Empty());

            return new TimeoutSettings
            {
                CheckListFileName = actionResponse.PdfPath
            };
        }
    }
}
