using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Testing;
using Ism.Recorder.Client.V1;
using Ism.Recorder.Core.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Ism.Recorder.Core.V1.Protos.Recorder;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RecorderService : IRecorderService
    {
        readonly IConfigurationService _configurationService;

        public bool UseMocks { get; set; }

        public RecorderSecureClient RecorderClient { get; set; }

        public RecorderService(IConfigurationService configurationService, IGrpcClientFactory<RecorderClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            UseMocks = true;

            if (UseMocks)
            {
                Mock<RecorderClient> mockGrpcClient = new Mock<RecorderClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.StartRecordingAsync(Moq.It.IsAny<RecordMessage>(), null, null, CancellationToken.None)).Returns(fakeCall);
                mockGrpcClient.Setup(mock => mock.StopRecordingAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                var mockFactory = new Mock<IGrpcClientFactory<RecorderClient>>();
                mockFactory.Setup(m => m.GetSecureClient(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<X509Certificate2>(), It.IsAny<string>(), It.IsAny<List<Interceptor>>(), It.IsAny<List<Func<Metadata, Metadata>>>()))
                        .Returns(mockGrpcClient.Object);

                grpcClientFactory = mockFactory.Object;
            }


            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            //RecorderClient = ClientHelper.GetInsecureClient<Recorder.RecorderClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
            RecorderClient = new RecorderSecureClient(grpcClientFactory, hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        public async Task StartRecording(RecordMessage recordMessage) => await RecorderClient.StartRecording(recordMessage);

        public async Task StopRecording() => await RecorderClient.StopRecording();
    }
}
