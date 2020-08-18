using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Broadcaster.Services;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Notifications
{
    public class NotificationsManager : INotificationsManager
    {
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IBroadcastService _broadcastService;
        readonly RabbitMqOptions _rabbitMqOptions;

        public NotificationsManager(IBroadcastService broadcastService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IRabbitMqClientService rabbitMqClient)
        {
            _rabbitMqClientService = rabbitMqClient;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _broadcastService = broadcastService;
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
