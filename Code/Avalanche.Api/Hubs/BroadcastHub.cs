using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Models;
using Ism.Broadcaster.Services;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ism.Broadcaster.Extensions;
using Ism.RabbitMq.Client;
using Avalanche.Api.Handlers;
using Ism.RabbitMq.Client.Models;
using Microsoft.Extensions.Options;

namespace Avalanche.Api.Hubs
{
    public class BroadcastHub : Hub
    {
        #region Constructor

        readonly IHubContext<BroadcastHub> _hubContext;
        readonly ILogger _appLoggerService;
        readonly RabbitMqOptions _rabbitMqOptions;

        public BroadcastHub(IBroadcastService broadcaster,
            IRabbitMqClientService rabbitMqClientService,
            ILogger<BroadcastHub> appLoggerService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IHubContext<BroadcastHub> hubContext)
        {
            _hubContext = hubContext;
            _appLoggerService = appLoggerService;
            _rabbitMqOptions = rabbitMqOptions.Value;

            if (broadcaster == null)
                throw new ArgumentNullException("Broadcast object is null!");

            BeginBroadcast(broadcaster, rabbitMqClientService); //This will avoid an explicit call to initialize a hub by message broadcaster client
        }

        #endregion

        #region Private Methods

        public override Task OnConnectedAsync()
        {
            ConnectionsHandler.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionsHandler.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private void BeginBroadcast(IBroadcastService broadcastService, IRabbitMqClientService rabbitMqClientService)
        {
            rabbitMqClientService.SubscribeToDirectMessages(_rabbitMqOptions.QueueName, (messageRequest, deliveryTag) =>
            {
                broadcastService.Broadcast((messageRequest.Json().Get<Ism.Broadcaster.Models.MessageRequest>()));
                //Here we can take the decision of preserve or discard the message from RabbitMq
                rabbitMqClientService.SetAcknowledge(deliveryTag, true);
            });

            // Register/Attach broadcast listener event
            broadcastService.MessageListened += (sender, broadcastArgs) =>
            {
                RegisterMessageEvents(broadcastArgs);
            };

            // Unregister/detach broadcast listener event
            broadcastService.MessageListened -= (sender, broadcastArgs) =>
            {
                RegisterMessageEvents(broadcastArgs);
            };
        }

        private void RegisterMessageEvents(BroadcastEventArgs broadcastArgs)
        {
            try
            {
                Ism.Broadcaster.Models.MessageRequest messageRequest = broadcastArgs.MessageRequest;

                if (broadcastArgs != null)
                {
                    IClientProxy clientProxy = _hubContext.Clients.All;

                    if (messageRequest.EventGroup == EventGroupEnum.Unknown)
                    {
                        string errorMessage = "Unknown or empty event name is requested!";
                        clientProxy.SendAsync(EventGroupEnum.OnException.EnumDescription(), errorMessage); // Goes to the listener
                        throw new Exception(errorMessage); // Goes to the broadcaster
                    }
                    else
                    {
                        clientProxy.SendAsync(messageRequest.EventGroup.EnumDescription(), messageRequest.Content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                _appLoggerService.LogError($"Error sending notification to the cloud boadcaster", ex);
            }
        }

        #endregion
    }
}
