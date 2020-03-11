using System;
using Avalanche.Api.Hubs;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.Extensions;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace IAvalanche.Api.Services.Dequeuer
{
    public class DequeuerService : IDequeuerService
    {
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IConfigurationService _configurationService;
        readonly IHubContext<BroadcastHub> _hubContext;
        readonly RabbitMqOptions _rabbitMqOptions;

        public bool IsEnabled { get; private set; }

        public DequeuerService(IRabbitMqClientService rabbitMqClientService, 
            IConfigurationService configurationService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IHubContext<BroadcastHub> hubContext)
        {
            _hubContext = hubContext;
            _rabbitMqClientService = rabbitMqClientService;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _configurationService = configurationService;
        }

        public void Initialize()
        {
            try
            {
                //TODO: Should we use only one queue?
                _rabbitMqClientService.SubscribeToDirectLogs(_rabbitMqOptions.QueueName, OnDirectLogReceived);

                IsEnabled = true;
            }
            catch (Exception ex)
            {
                IsEnabled = false;
            }
        }

        public void OnDirectLogReceived(Message messageRequest, ulong deliveryTag)
        {
            IClientProxy clientProxy = _hubContext.Clients.All;

            if (messageRequest.EventName.Equals(EventNameEnum.Unknown))
            {
                string errorMessage = "Unknown or empty event name is requested!";
                clientProxy.SendAsync(EventNameEnum.OnException.EnumDescription(), errorMessage); // Goes to the listener
                throw new Exception(errorMessage); // Goes to the broadcaster
            }
            else
            {
                clientProxy.SendAsync(messageRequest.EventName.EnumDescription(), messageRequest.Content);
            }

            _rabbitMqClientService.SetAcknowledge(deliveryTag, true);
        }
    }
}
