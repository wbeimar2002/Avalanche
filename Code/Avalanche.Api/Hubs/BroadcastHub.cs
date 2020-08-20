using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Hubs
{
    [ExcludeFromCodeCoverage]
    public class BroadcastHub : Hub
    {
        #region Constructor

        readonly ILogger _appLoggerService;
        readonly RabbitMqOptions _rabbitMqOptions;
        readonly IBroadcastService _broadcastService;
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IHubContext<BroadcastHub> _hubContext;

        public BroadcastHub(IHubContext<BroadcastHub> hubContext,
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

            if (broadcastService == null)
                throw new ArgumentNullException("BroadCast object is null !");

            BeginBroadcast(); // This will avoid an explicit call to initialize a hub by message broadcaster client

            _rabbitMqClientService.SubscribeToDirectMessages(_rabbitMqOptions.QueueName, ProcessMessage);
        }

        #endregion

        #region Private Methods

        private void ProcessMessage(Ism.RabbitMq.Client.Models.MessageRequest messageRequest, ulong deliveryTag)
        {
            var message = (messageRequest.Json().Get<Ism.Broadcaster.Models.MessageRequest>());
            _broadcastService.Broadcast(message);
            //Here we can take the decision of preserve or discard the message from RabbitMq
            _rabbitMqClientService.SetAcknowledge(deliveryTag, true);
        }

        /// <summary>
        /// Begin broadCast message
        /// </summary>
        /// <param name="broadCast">IBroadCast value</param>
        private void BeginBroadcast()
        {
            // Register/Attach broadcast listener event
            _broadcastService.MessageListened += (sender, broadCastArgs)
                =>
            {
                RegisterMessageEvents(broadCastArgs);
            };

            // Unregister/detach broadcast listener event
            _broadcastService.MessageListened -= (sender, broadCastArgs)
               =>
            {
                RegisterMessageEvents(broadCastArgs);
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

                    if (string.IsNullOrEmpty(messageRequest.EventName))
                    {
                        string errorMessage = "Unknown or empty event name is requested!";
                        clientProxy.SendAsync("OnException", errorMessage); // Goes to the listener
                        throw new Exception(errorMessage); // Goes to the broadcaster
                    }
                    else
                    {
                        clientProxy.SendAsync(messageRequest.EventName, messageRequest.Content);
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
