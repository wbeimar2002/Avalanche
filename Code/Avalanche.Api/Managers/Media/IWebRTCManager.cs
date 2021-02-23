using Avalanche.Shared.Domain.Models.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IWebRTCManager
    {
        Task<List<string>> InitSessionAsync(WebRTCSessionModel session);
        Task DeInitSessionAsync(WebRTCMessaggeModel message);
        Task<IList<VideoDeviceModel>> GetSourceStreams();
        Task HandleMessageForVideo(WebRTCMessaggeModel message);
    }
}