using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.Procedure;
using Ism.SystemState.Models.VideoRouting;
using Ism.SystemState.Models.PgsTimeout;
using System.Threading.Tasks;

namespace Avalanche.Api.Hubs
{
    public interface IBroadcastHubClient
    {
        Task SendGeneric(string eventName, string content);

        Task OnImageCapture(ImageCapturedEvent imageCapturedEvent);

        Task OnVideoSourceStateChanged(VideoSourceStateChangedEvent videoSourceStateChangedEvent);

        Task OnVideoSourceIdentityChanged(VideoSourceIdentityChangedEvent videoSourceIdentityChangedEvent);

        Task OnVideoSinkSourceChanged(VideoSinkSourceChangedEvent videoSinkSourceChangedEvent);

        Task OnActiveProcedureStateChanged(ActiveProcedureViewModel activeProcedureState);

        Task OnPgsDisplayStateDataChanged(PgsDisplayStateData pgsDisplayStateData);

        Task OnPgsTimeoutPlayerDataChanged(PgsTimeoutPlayerData pgsTimeoutPlayerData);
    }
}
