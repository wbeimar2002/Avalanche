using System;
using Avalanche.Api.Hubs;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.Extensions;
using Ism.Broadcaster.Services;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Broadcaster.Models;

namespace IAvalanche.Api.Services.Dequeuer
{
    public class DequeuerService : IDequeuerService
    {
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IConfigurationService _configurationService;
        readonly IBroadcastService _broadcastService;
        readonly IHubContext<BroadcastHub> _hubContext;
        readonly RabbitMqOptions _rabbitMqOptions;

        public bool IsEnabled { get; private set; }

        public DequeuerService(IRabbitMqClientService rabbitMqClientService, 
            IConfigurationService configurationService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IBroadcastService broadcastService,
            IHubContext<BroadcastHub> hubContext)
        {
            _hubContext = hubContext;
            _broadcastService = broadcastService;
            _rabbitMqClientService = rabbitMqClientService;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _configurationService = configurationService;
        }

        public void Initialize()
        {
            try
            {
                //TODO: Should we use only one queue?
                _rabbitMqClientService.SubscribeToDirectMessages(_rabbitMqOptions.QueueName, OnDirectMessageReceived);

                IsEnabled = true;
            }
            catch (Exception ex)
            {
                IsEnabled = false;
            }
        }

        public void OnDirectMessageReceived(Ism.RabbitMq.Client.Models.MessageRequest messageRequest, ulong deliveryTag)
        {
            _broadcastService.Broadcast(JsonConvert.DeserializeObject<Ism.Broadcaster.Models.MessageRequest>(messageRequest.Json()));
            //Here we can take the decision of preserve or discard the message from RabbitMq
            _rabbitMqClientService.SetAcknowledge(deliveryTag, true);
        }
    }
}
