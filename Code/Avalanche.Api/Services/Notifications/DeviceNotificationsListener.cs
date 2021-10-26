using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Hubs;
using Avalanche.Api.Managers.Media;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Exceptions;
using Ism.SystemState.Models.Library;
using Ism.SystemState.Models.Medpresence;
using Ism.SystemState.Models.Notifications;
using Ism.SystemState.Models.PgsTimeout;
using Ism.SystemState.Models.Procedure;
using Ism.SystemState.Models.Recorder;
using Ism.SystemState.Models.VideoRouting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Services.Notifications
{
    public class DeviceNotificationsListener : NotificationsListener
    {
        private readonly IHubContext<BroadcastHub, IBroadcastHubClient> _hubContext;
        private readonly IMapper _mapper;
        private readonly IRoutingManager _routingManager;

        public DeviceNotificationsListener(IBroadcastService broadcastService,
            IHubContext<BroadcastHub, IBroadcastHubClient> hubContext,
            ILogger<NotificationsListener> logger,
            IStateClient stateClient,
            IMapper mapper,
            IRoutingManager routingManager):base(broadcastService, hubContext, stateClient, logger)
        {
            _hubContext = hubContext;
            _mapper = mapper;
            _routingManager = routingManager;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            BeginBroadcast();

            AddSubscription<VideoSourceStateChangedEvent>(evt => _hubContext.Clients.All.OnVideoSourceStateChanged(evt));
            AddSubscription<VideoSourceIdentityChangedEvent>(evt => _hubContext.Clients.All.OnVideoSourceIdentityChanged(evt));
            AddSubscription<VideoSinkSourceChangedEvent>(evt =>
            {
                _hubContext.Clients.All.OnVideoSinkSourceChanged(evt);
                _routingManager.HandleSinkSourceChanged(
                    _mapper.Map<AliasIndexModel, Shared.Domain.Models.Media.AliasIndexModel>(evt.Sink),
                    _mapper.Map<AliasIndexModel, Shared.Domain.Models.Media.AliasIndexModel>(evt.Source));
            });

            AddSubscription<DiskSpaceEvent>(evt => _hubContext.Clients.All.OnDiskSpaceStateChanged(evt));

            AddDataSubscription<ActiveProcedureState>(data =>
            {
                var mapped = _mapper.Map<ViewModels.ActiveProcedureViewModel>(data);
                _hubContext.Clients.All.OnActiveProcedureStateChanged(mapped);
            });

            AddDataSubscription<PgsDisplayStateData>(data => _hubContext.Clients.All.OnPgsDisplayStateDataChanged(data));
            AddDataSubscription<PgsTimeoutPlayerData>(data => _hubContext.Clients.All.OnPgsTimeoutPlayerDataChanged(data));
            AddDataSubscription<DisplayRecordStateData>(data => _hubContext.Clients.All.OnDisplayBasedRecordingStateDataChanged(data));

            AddSubscription<RecorderStateEvent>(evt => _hubContext.Clients.All.OnRecorderStateChanged(evt));
            AddSubscription<TimeoutStateData>(evt => _hubContext.Clients.All.OnTimeoutStateDataChanged(evt));
            AddSubscription<SystemErrorRaisedEvent>(evt => _hubContext.Clients.All.OnSystemErrorRaised(evt));
            AddSubscription<SystemPersistantNotificationRaisedEvent>(evt => _hubContext.Clients.All.OnSystemSystemPersistantNotificationRaised(evt));

            AddSubscription<PgsTimeoutRoomStateEvent>(evt => _hubContext.Clients.All.OnPgsTimeoutRoomStateChanged(evt));

            AddSubscription<ImageCaptureStartedEvent>(evt => _hubContext.Clients.All.OnImageCaptureStarted(evt));

            AddDataSubscription<VideoRoutingStateData>(data => _hubContext.Clients.All.OnVideoRoutingStateDataChanged(data));

            AddDataSubscription<MedpresenceState>(evt => _hubContext.Clients.All.OnMedpresenceStateDataChanged(evt));

            AddSubscription<ImageCaptureSucceededEvent>(evt => _hubContext.Clients.All.OnImageCaptureSucceeded(evt));

            return Task.CompletedTask;
        }
    }
}
