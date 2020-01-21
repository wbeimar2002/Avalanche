using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Broadcaster.Enumerations;
using Avalanche.Api.Broadcaster.EventArgs;
using Avalanche.Api.Broadcaster.Models;
using Avalanche.Api.Broadcaster.Services;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Services.Logger;
using Microsoft.AspNetCore.SignalR;

namespace Avalanche.Api.Hubs
{
    public class BroadcastHub : Hub
    {
        #region Constructor

        private readonly IHubContext<BroadcastHub> _hubContext;
        private readonly IAppLoggerService _appLoggerService;

        public BroadcastHub(IBroadcastService broadcaster,
            IAppLoggerService appLoggerService,
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
                if (broadcastArgs != null)
                {
                    MessageRequest messageRequest = broadcastArgs.MessageRequest;

                    IClientProxy clientProxy = _hubContext.Clients.All;

                    if (messageRequest.EventName == EventNameEnum.Unknown)
                    {
                        string errorMessage = "Unknown or empty event name is requested!";
                        clientProxy.SendAsync(EventNameEnum.OnException.EnumDescription(), errorMessage); // Goes to the listener
                        throw new Exception(errorMessage); // Goes to the broadcaster
                    }
                    else
                    {
                        clientProxy.SendAsync(messageRequest.EventName.EnumDescription(), messageRequest.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                _appLoggerService.Log(LogType.Error, $"Error sending notification to the cloud boadcaster", ex);
            }
        }

        #endregion
    }
}
