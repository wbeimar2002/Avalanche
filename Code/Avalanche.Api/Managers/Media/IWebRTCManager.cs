using System.Threading.Tasks;
using Avalanche.Shared.Domain.Models.Media;

namespace Avalanche.Api.Managers.Media
{
    // maps to the public interface exposed in media service
    public interface IWebRtcManager
    {
        Task<InitWebRtcSessionResponse> InitSession(InitWebRtcSessionRequest request);
        Task HandleMessage(HandleWebRtcMessageRequest request);
        Task DeInitSession(DeInitWebRtcSessionRequest request);
        Task<GetWebRtcStreamsResponse> GetSourceStreams();
    }
}
