using Ism.SystemState.Models.Procedure;
using Ism.SystemState.Models.VideoRouting;
using System.Threading.Tasks;

namespace Avalanche.Api.Hubs
{
    public interface IBroadcastHubClient
    {
        Task SendGeneric(string eventName, string content);

        Task OnImageCapture(ImageCapturedEvent imageCapturedEvent);

        Task OnVideoSourceStateChanged(VideoSourceStateChangedEvent videoSourceStateChangedEvent);

        Task OnVideoSourceIdentityChanged(VideoSourceIdentityChangedEvent videoSourceIdentityChangedEvent);
    }
}
