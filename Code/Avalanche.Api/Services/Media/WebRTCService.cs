using Ism.Streaming.Client.V1;
using Ism.Streaming.V1.Protos;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public partial class WebRtcService : IWebRtcService
    {
        private readonly WebRtcStreamerSecureClient _client;

        public WebRtcService(WebRtcStreamerSecureClient client) => _client = ThrowIfNullOrReturn(nameof(client), client);

        public async Task DeInitSessionAsync(DeInitSessionRequest deInitSessionRequest) => await _client.DeInitSession(deInitSessionRequest).ConfigureAwait(false);

        public async Task<GetSourceStreamsResponse> GetSourceStreamsAsync() => await _client.GetSourceStreams().ConfigureAwait(false);

        public async Task HandleMessageAsync(HandleMessageRequest handleMessageRequest) => await _client.HandleMessage(handleMessageRequest).ConfigureAwait(false);

        public async Task<InitSessionResponse> InitSessionAsync(InitSessionRequest initSessionRequest) => await _client.InitSession(initSessionRequest).ConfigureAwait(false);
    }
}
