using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalanche.Api.Hubs;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Services.Notifications
{
    public abstract class NotificationsListener : IHostedService
    {
        // TODO: Is the IBroadcastService stuff still necessary? Discuss.
        private readonly IBroadcastService _broadcastService;
        private readonly IHubContext<BroadcastHub, IBroadcastHubClient> _hubContext;
        private readonly ILogger _logger;
        private readonly IStateClient _stateClient;

        private List<Guid> _subscriptions = new List<Guid>();

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

        public abstract Task StartAsync(CancellationToken cancellationToken);

        protected void AddSubscription<TEvent>(Action<TEvent> handler)
            where TEvent : StateEvent
        {
            var id = _stateClient.SubscribeEvent<TEvent>(handler);
            _subscriptions.Add(id);
        }

        protected void AddDataSubscription<TData>(Action<TData> handler)
            where TData : StateData
        {
            var id = _stateClient.SubscribeData<TData>(handler);
            _subscriptions.Add(id);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Unregister/detach broadcast listener event
            _broadcastService.MessageListened -= RegisterMessageEvents;

            foreach (var id in _subscriptions)
            {
                _stateClient?.Unsubscribe(id);
            }
            _subscriptions.Clear();

            return Task.CompletedTask;
        }

        protected void BeginBroadcast()
        {
            // Register/Attach broadcast listener event
            _broadcastService.MessageListened += RegisterMessageEvents;
        }

        protected void RegisterMessageEvents(object sender, BroadcastEventArgs broadcastArgs)
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
