using Avalanche.Api.Hubs;
using Ism.Broadcaster.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Managers.Notifications
{
    public class NotificationsManager : INotificationsManager
    {
        readonly ILogger _logger;
        readonly IBroadcastService _broadcastService;
        readonly IHubContext<BroadcastHub> _hubContext;

        public NotificationsManager(IHubContext<BroadcastHub> hubContext,
            IBroadcastService broadcastService,
            ILogger<BroadcastHub> logger)
        {
            _broadcastService = broadcastService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public void SendDirectMessage(Ism.Broadcaster.Models.MessageRequest messageRequest) =>
            _broadcastService.Broadcast(messageRequest);
    }
}
