using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Interfaces;
using Ism.Streaming.Client.V1;
using Ism.Streaming.V1.Protos;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.Streaming.V1.Protos.WebRtcStreamer;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public partial class WebRTCService : IWebRTCService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        WebRtcStreamerSecureClient WebRtcStreamerClient { get; set; }

        public WebRTCService(IConfigurationService configurationService, 
            IGrpcClientFactory<WebRtcStreamerClient> grpcClientFactory, 
            ICertificateProvider certificateProvider)
        {
            _configurationService = ThrowIfNullOrReturn(nameof(configurationService), configurationService);
            ThrowIfNull(nameof(grpcClientFactory), grpcClientFactory);
            ThrowIfNull(nameof(certificateProvider), certificateProvider);

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            WebRtcStreamerClient = new WebRtcStreamerSecureClient(grpcClientFactory, _hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        #region WebRTC
        
        public async Task<GetSourceStreamsResponse> GetSourceStreamsAsync() => await WebRtcStreamerClient.GetSourceStreams();

        public Task HandleMessageAsync(HandleMessageRequest handleMessageRequest) => WebRtcStreamerClient.HandleMessage(handleMessageRequest);

        public async Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(InitSessionRequest initSessionRequest) => await WebRtcStreamerClient.InitSession(initSessionRequest);

        public Task DeInitSessionAsync(DeInitSessionRequest deInitSessionRequest) => WebRtcStreamerClient.DeInitSession(deInitSessionRequest);

        #endregion WebRTC
    }
}
