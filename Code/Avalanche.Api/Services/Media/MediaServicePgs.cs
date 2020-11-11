using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Client.V1;
using Ism.PgsTimeout.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using Ism.Streaming.Client.V1;
using Ism.Streaming.V1.Protos;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Streaming.V1.Protos.WebRtcStreamer;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        readonly IConfigurationService _configurationService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly string _hostIpAddress;


        public WebRtcStreamerSecureClient WebRtcStreamerClient { get; set; }

        public MediaService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory, IGrpcClientFactory<WebRtcStreamerClient> grpcWebrtcClientFactory, IGrpcClientFactory<PgsTimeoutClient> grpcPgsClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var PgsTimeoutGrpcPort = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new X509Certificate2(grpcCertificate, grpcPassword);
            UseMocks = true;

            if (UseMocks)
            {
                Mock<PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);
                mockGrpcClient.Setup(mock => mock.SetTimeoutPageAsync(Moq.It.IsAny<SetTimeoutPageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);
                mockGrpcClient.Setup(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);
                mockGrpcClient.Setup(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                var mockPgs = new Mock<IGrpcClientFactory<PgsTimeoutClient>>();
                mockPgs.Setup(m => m.GetSecureClient(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<X509Certificate2>(), It.IsAny<string>(), It.IsAny<List<Interceptor>>(), It.IsAny<List<Func<Metadata, Metadata>>>()))
                        .Returns(mockGrpcClient.Object);

                grpcPgsClientFactory = mockPgs.Object;
            }

            PgsTimeoutClient = new PgsTimeoutSecureClient(grpcPgsClientFactory, _hostIpAddress, PgsTimeoutGrpcPort, certificateProvider);
            WebRtcStreamerClient = new WebRtcStreamerSecureClient(grpcWebrtcClientFactory, _hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        #region WebRTC

        public async Task<GetSourceStreamsResponse> GetSourceStreamsAsync() => await WebRtcStreamerClient.GetSourceStreams();

        public Task HandleMessageAsync(HandleMessageRequest handleMessageRequest) => WebRtcStreamerClient.HandleMessage(handleMessageRequest);

        public async Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(InitSessionRequest initSessionRequest) => await WebRtcStreamerClient.InitSession(initSessionRequest);

        public Task DeInitSessionAsync(DeInitSessionRequest deInitSessionRequest) => WebRtcStreamerClient.DeInitSession(deInitSessionRequest);

        #endregion WebRTC
    }
}
