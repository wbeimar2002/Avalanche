using Avalanche.Api.Hubs;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Notifications
{
    public class NotificationsListener : IHostedService
    {
        // TODO: Is the IBroadcastService stuff still necessary? Discuss.
        private readonly IBroadcastService _broadcastService;
        private readonly IHubContext<BroadcastHub, IBroadcastHubClient> _hubContext;
        private readonly ILogger _logger;
        private readonly IStateClient _stateClient;

        public NotificationsListener(
            IBroadcastService broadcastService,
            IHubContext<BroadcastHub, IBroadcastHubClient> hubContext,
            IStateClient stateClient,
            ILogger<NotificationsListener> logger)
        {
            _broadcastService = broadcastService;
            _hubContext = hubContext;
            _stateClient = stateClient;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            BeginBroadcast();
            _stateClient.SubscribeEvent<ImageCapturedEvent>(evt => _hubContext.Clients.All.OnImageCapture(evt));
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Unregister/detach broadcast listener event
            _broadcastService.MessageListened -= RegisterMessageEvents;
            _stateClient?.UnsubscribeEvent<ImageCapturedEvent>();

            return Task.CompletedTask;
        }

        private void BeginBroadcast()
        {
            // Register/Attach broadcast listener event
            _broadcastService.MessageListened += RegisterMessageEvents;
        }

        private void RegisterMessageEvents(object sender, BroadcastEventArgs broadcastArgs)
        {
            try
            {
                Ism.Broadcaster.Models.MessageRequest messageRequest = broadcastArgs.MessageRequest;

                if (broadcastArgs != null)
                {
                    var clientProxy = _hubContext.Clients.All;

                    if (string.IsNullOrEmpty(messageRequest.EventName))
                    {
                        string errorMessage = "Unknown or empty event name is requested!";
                        clientProxy.SendGeneric("OnException", errorMessage); // Goes to the listener
                        throw new Exception(errorMessage); // Goes to the broadcaster
                    }
                    else
                    {
                        clientProxy.SendGeneric(messageRequest.EventName, messageRequest.Content);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to the cloud boadcaster");
            }
        }
    }
}
