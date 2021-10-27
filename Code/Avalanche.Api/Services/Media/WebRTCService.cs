using Ism.Streaming.Client.V1;
using Ism.Streaming.V1.Protos;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public partial class WebRtcService : IWebRTCService
    {
        private readonly WebRtcStreamerSecureClient _client;

        public WebRtcService(WebRtcStreamerSecureClient client) => _client = client;

        public Task DeInitSessionAsync(DeInitSessionRequest deInitSessionRequest) => _client.DeInitSession(deInitSessionRequest);

        public async Task<GetSourceStreamsResponse> GetSourceStreamsAsync() => await _client.GetSourceStreams().ConfigureAwait(false);

        public Task HandleMessageAsync(HandleMessageRequest handleMessageRequest) => _client.HandleMessage(handleMessageRequest);

        public async Task<InitSessionResponse> InitSessionAsync(InitSessionRequest initSessionRequest) => await _client.InitSession(initSessionRequest).ConfigureAwait(false);
    }
}
