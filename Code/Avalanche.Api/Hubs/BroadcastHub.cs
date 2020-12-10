using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Models;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Hubs
{
    [ExcludeFromCodeCoverage]
    [Authorize]
    public class BroadcastHub : Hub, IDisposable
    {
        public const string BroadcastHubRoute = "/broadcast";

        #region Constructor

        readonly ILogger _appLoggerService;
        readonly IBroadcastService _broadcastService;
        readonly IHubContext<BroadcastHub> _hubContext;
        readonly IStateClient _stateClient;

        public BroadcastHub(IHubContext<BroadcastHub> hubContext,
            IBroadcastService broadcastService,
            IStateClient stateClient,
            ILogger<BroadcastHub> appLoggerService)
        {
            _broadcastService = broadcastService;
            _hubContext = hubContext;
            _appLoggerService = appLoggerService;

            _stateClient = stateClient;

            if (broadcastService == null)
                throw new ArgumentNullException("BroadCast object is null !");

            BeginBroadcast(); // This will avoid an explicit call to initialize a hub by message broadcaster client
            RegisterStateEvents();
        }

        #endregion

        public void Dispose()
        {
            // Unregister/detach broadcast listener event
            _broadcastService.MessageListened -= RegisterMessageEvents;
        }

        #region Private Methods

        /// <summary>
        /// Subscribe to any state data/events/topics we care about. At Avalanche.Api layer, this is probably just always going to be passthrough via SignalR to Avalanche.Web.
        /// </summary>
        private void RegisterStateEvents()
        {
            _stateClient.SubscribeTopic(new ProcedureTopicHandler
            {
                OnImageCaptured = (evt) => ProcessMessage(evt, nameof(ImageCapturedEvent))
            });
        }

        private void ProcessMessage(object data, string eventName)
        {
            var messageJson = data.Json();
            var messageRequest = new MessageRequest
            {
                Content = messageJson,
                EventName = eventName
            };

            _broadcastService.Broadcast(messageRequest);
        }

        /// <summary>
        /// Begin broadCast message
        /// </summary>
        /// <param name="broadCast">IBroadCast value</param>
        private void BeginBroadcast()
        {
            // Register/Attach broadcast listener event
            _broadcastService.MessageListened += RegisterMessageEvents;
        }

        private void RegisterMessageEvents(object sender, BroadcastEventArgs broadcastArgs)
        {
            try
            {
                MessageRequest messageRequest = broadcastArgs.MessageRequest;

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
                _appLoggerService.LogError($"Error sending notification via boadcaster", ex);
            }
        }

        #endregion
    }
}
