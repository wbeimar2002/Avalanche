using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.VideoRouting;
using Ism.SystemState.Models.PgsTimeout;
using System.Threading.Tasks;
using Ism.SystemState.Models.Library;
using Ism.SystemState.Models.Exceptions;
using Ism.SystemState.Models.Recorder;
using Ism.SystemState.Models.Notifications;

namespace Avalanche.Api.Hubs
{
    public interface IBroadcastHubClient
    {
        Task SendGeneric(string eventName, string content);

        Task OnVideoSourceStateChanged(VideoSourceStateChangedEvent videoSourceStateChangedEvent);

        Task OnVideoSourceIdentityChanged(VideoSourceIdentityChangedEvent videoSourceIdentityChangedEvent);

        Task OnVideoSinkSourceChanged(VideoSinkSourceChangedEvent videoSinkSourceChangedEvent);

        Task OnActiveProcedureStateChanged(ActiveProcedureViewModel activeProcedureState);

        Task OnPgsDisplayStateDataChanged(PgsDisplayStateData pgsDisplayStateData);

        Task OnPgsTimeoutPlayerDataChanged(PgsTimeoutPlayerData pgsTimeoutPlayerData);

        Task OnDiskSpaceStateChanged(DiskSpaceEvent diskSpaceEvent);

        Task OnSystemErrorRaised(SystemErrorRaisedEvent evt);
        Task OnSystemSystemPersistantNotificationRaised(SystemPersistantNotificationRaisedEvent evt);

        Task OnRecorderStateChanged(RecorderStateEvent evt);
    }
}
