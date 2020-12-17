using Avalanche.Api.Hubs;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Broadcaster.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avalanche.Api.Managers.Notifications
{
    public class NotificationsManager : INotificationsManager
    {
        readonly ILogger _appLoggerService;
        readonly IBroadcastService _broadcastService;
        readonly IHubContext<BroadcastHub> _hubContext;

        public NotificationsManager(IHubContext<BroadcastHub> hubContext,
            IBroadcastService broadcastService,
            ILogger<BroadcastHub> appLoggerService)
        {
            _broadcastService = broadcastService;
            _hubContext = hubContext;
            _appLoggerService = appLoggerService;
        }

        public void SendDirectMessage(Ism.Broadcaster.Models.MessageRequest messageRequest)
        {
            _broadcastService.Broadcast(messageRequest);
        }
    }
}
