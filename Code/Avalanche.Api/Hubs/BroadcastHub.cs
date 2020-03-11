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

namespace Avalanche.Api.Hubs
{
    public class BroadcastHub : Hub
    {
        #region Constructor

        private readonly IHubContext<BroadcastHub> _hubContext;
        private readonly ILogger _appLoggerService;

        public BroadcastHub(IBroadcastService broadcaster,
            ILogger<BroadcastHub> appLoggerService,
            IHubContext<BroadcastHub> hubContext)
        {
            _hubContext = hubContext;
            _appLoggerService = appLoggerService;

            if (broadcaster == null)
                throw new ArgumentNullException("Broadcast object is null!");

            BeginBroadcast(broadcaster); //This will avoid an explicit call to initialize a hub by message broadcaster client
        }

        #endregion

        #region Private Methods

        private void BeginBroadcast(IBroadcastService broadcaster)
        {
            // Register/Attach broadcast listener event
            broadcaster.MessageListened += (sender, broadcastArgs) =>
            {
                RegisterMessageEvents(broadcastArgs);
            };

            // Unregister/detach broadcast listener event
            broadcaster.MessageListened -= (sender, broadcastArgs) =>
            {
                RegisterMessageEvents(broadcastArgs);
            };
        }

        private void RegisterMessageEvents(BroadcastEventArgs broadcastArgs)
        {
            try
            {
                MessageRequest messageRequest = broadcastArgs.MessageRequest;

                if (broadcastArgs != null)
                {
                    if (broadcastArgs.ExternalAction == null)
                    {
                        IClientProxy clientProxy = _hubContext.Clients.All;

                        if (messageRequest.EventName == EventNameEnum.Unknown)
                        {
                            string errorMessage = "Unknown or empty event name is requested!";
                            clientProxy.SendAsync(EventNameEnum.OnException.EnumDescription(), errorMessage); // Goes to the listener
                            throw new Exception(errorMessage); // Goes to the broadcaster
                        }
                        else
                        {
                            clientProxy.SendAsync(messageRequest.EventName.EnumDescription(), messageRequest.Content);
                        }
                    }
                    else
                    {
                        broadcastArgs.ExternalAction(messageRequest);
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
