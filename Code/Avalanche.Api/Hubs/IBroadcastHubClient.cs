using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.VideoRouting;
using Ism.SystemState.Models.PgsTimeout;
using System.Threading.Tasks;
using Ism.SystemState.Models.Library;
using Ism.SystemState.Models.Exceptions;
using Ism.SystemState.Models.Recorder;
using Ism.SystemState.Models.Notifications;
using Ism.SystemState.Models.Medpresence;

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

        Task OnDisplayBasedRecordingStateDataChanged(DisplayRecordStateData displayRecordStateData);

        Task OnDiskSpaceStateChanged(DiskSpaceEvent diskSpaceEvent);

        Task OnSystemErrorRaised(SystemErrorRaisedEvent evt);

        Task OnSystemSystemPersistantNotificationRaised(SystemPersistantNotificationRaisedEvent evt);

        Task OnRecorderStateChanged(RecorderStateEvent evt);

        Task OnTimeoutStateDataChanged(TimeoutStateData evt);

        Task OnPgsTimeoutRoomStateChanged(PgsTimeoutRoomStateEvent evt);

        Task OnImageCaptureStarted(ImageCaptureStartedEvent evt);

        Task OnSelectedSourceStateDataChanged(SelectedSourceStateData selectedSourceStateData);

        Task OnImageCaptureSucceeded(ImageCaptureSucceededEvent evt);
    }
}
