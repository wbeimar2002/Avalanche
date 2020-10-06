using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.Recorder.Common.Core;
using Ism.Security.Grpc.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class RecorderService : IRecorderService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool UseMocks { get; set; }

        public Recorder.RecorderClient RecorderClient { get; set; }

        public RecorderService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            UseMocks = true;

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            RecorderClient = ClientHelper.GetInsecureClient<Recorder.RecorderClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        public async Task StartRecording()
        {
            //Faking calls while I have the real server
            if (UseMocks)
            {
                Mock<Recorder.RecorderClient> mockGrpcClient = new Mock<Recorder.RecorderClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.StartRecordingAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                RecorderClient = mockGrpcClient.Object;
            }

            await RecorderClient.StartRecordingAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task StopRecording()
        {
            if (UseMocks)
            {
                Mock<Recorder.RecorderClient> mockGrpcClient = new Mock<Recorder.RecorderClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.StopRecordingAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                RecorderClient = mockGrpcClient.Object;
            }

            await RecorderClient.StopRecordingAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
