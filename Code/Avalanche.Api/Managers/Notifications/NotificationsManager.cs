using Avalanche.Api.Hubs;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Notifications
{
    public class NotificationsManager : INotificationsManager
    {
        readonly ILogger _appLoggerService;
        readonly RabbitMqOptions _rabbitMqOptions;
        readonly IBroadcastService _broadcastService;
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IHubContext<BroadcastHub> _hubContext;

        public NotificationsManager(IHubContext<BroadcastHub> hubContext,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IBroadcastService broadcastService,
            IRabbitMqClientService rabbitMqClientService,
            ILogger<BroadcastHub> appLoggerService)
        {
            _broadcastService = broadcastService;
            _rabbitMqClientService = rabbitMqClientService;
            _hubContext = hubContext;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _appLoggerService = appLoggerService;
        }

        public void SendDirectMessage(Ism.Broadcaster.Models.MessageRequest messageRequest)
        {
            _broadcastService.Broadcast(messageRequest);
        }

        public void SendQueuedMessage(Ism.Broadcaster.Models.MessageRequest messageRequest)
        {
            _rabbitMqClientService.SendMessage(_rabbitMqOptions.QueueName, messageRequest.Json());
        }
    }
}
