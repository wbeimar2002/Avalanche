using System.Threading.Tasks;
using Ism.Streaming.V1.Protos;

namespace Avalanche.Api.Services.Media
{
    /// <summary>
    /// Thin wrapper around the webrtc gRPC client
    /// </summary>
    public interface IWebRtcService
    {
        Task<GetSourceStreamsResponse> GetSourceStreamsAsync();
        Task HandleMessageAsync(HandleMessageRequest handleMessageRequest);
        Task<InitSessionResponse> InitSessionAsync(InitSessionRequest initSessionRequest);
        Task DeInitSessionAsync(DeInitSessionRequest deInitSessionRequest);
    }
}
