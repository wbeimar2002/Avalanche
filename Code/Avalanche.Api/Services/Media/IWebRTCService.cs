using Ism.PgsTimeout.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IWebRTCService
    {
        Task<Ism.Streaming.V1.Protos.GetSourceStreamsResponse> GetSourceStreamsAsync();
        Task HandleMessageAsync(Ism.Streaming.V1.Protos.HandleMessageRequest handleMessageRequest);
        Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(Ism.Streaming.V1.Protos.InitSessionRequest initSessionRequest);
        Task DeInitSessionAsync(Ism.Streaming.V1.Protos.DeInitSessionRequest deInitSessionRequest);
    }
}
