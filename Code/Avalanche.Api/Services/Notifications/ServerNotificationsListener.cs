using System.Threading;
using System.Threading.Tasks;
using Avalanche.Api.Hubs;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Exceptions;
using Ism.SystemState.Models.Library;
using Ism.SystemState.Models.Medpresence;
using Ism.SystemState.Models.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Services.Notifications
{
    public class ServerNotificationsListener : NotificationsListener
    {
        private readonly IHubContext<BroadcastHub, IBroadcastHubClient> _hubContext;

        public ServerNotificationsListener(IBroadcastService broadcastService,
            IHubContext<BroadcastHub, IBroadcastHubClient> hubContext,
            ILogger<NotificationsListener> logger,
            IStateClient stateClient) : base(broadcastService, hubContext, stateClient, logger)
        {
            _hubContext = hubContext;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            BeginBroadcast();

            AddSubscription<DiskSpaceEvent>(evt => _hubContext.Clients.All.OnDiskSpaceStateChanged(evt));
            AddSubscription<SystemErrorRaisedEvent>(evt => _hubContext.Clients.All.OnSystemErrorRaised(evt));
            AddSubscription<SystemPersistantNotificationRaisedEvent>(evt => _hubContext.Clients.All.OnSystemSystemPersistantNotificationRaised(evt));
            AddDataSubscription<MedpresenceState>(evt => _hubContext.Clients.All.OnMedpresenceStateDataChanged(evt));

            return Task.CompletedTask;
        }
    }
}
