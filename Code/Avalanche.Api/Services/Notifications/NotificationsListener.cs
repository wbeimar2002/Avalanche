using AutoMapper;
using Avalanche.Api.Hubs;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using Ism.SystemState.Client;
using Ism.SystemState.Models;
using Ism.SystemState.Models.Procedure;
using Ism.SystemState.Models.VideoRouting;
using Ism.SystemState.Models.PgsTimeout;
using Ism.SystemState.Models.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ism.SystemState.Models.Library;
using Ism.SystemState.Models.Recorder;
using Ism.SystemState.Models.Notifications;
using Ism.SystemState.Models.Medpresence;
using Avalanche.Api.Managers.Media;

namespace Avalanche.Api.Services.Notifications
{
    public class NotificationsListener : IHostedService
    {
        // TODO: Is the IBroadcastService stuff still necessary? Discuss.
        private readonly IBroadcastService _broadcastService;
        private readonly IHubContext<BroadcastHub, IBroadcastHubClient> _hubContext;
        private readonly ILogger _logger;
        private readonly IStateClient _stateClient;
        private readonly IMapper _mapper;
        private readonly IRoutingManager _routingManager;

        private List<Guid> _subscriptions = new List<Guid>();

        public NotificationsListener(
            IBroadcastService broadcastService,
            IHubContext<BroadcastHub, IBroadcastHubClient> hubContext,
            IStateClient stateClient,
            ILogger<NotificationsListener> logger,
            IMapper mapper,
            IRoutingManager routingManager)
        {
            _broadcastService = broadcastService;
            _hubContext = hubContext;
            _stateClient = stateClient;
            _logger = logger;
            _mapper = mapper;
            _routingManager = routingManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            BeginBroadcast();

            AddSubscription<VideoSourceStateChangedEvent>(evt => _hubContext.Clients.All.OnVideoSourceStateChanged(evt));
            AddSubscription<VideoSourceIdentityChangedEvent>(evt => _hubContext.Clients.All.OnVideoSourceIdentityChanged(evt));
            AddSubscription<VideoSinkSourceChangedEvent>(evt =>
            {
                _hubContext.Clients.All.OnVideoSinkSourceChanged(evt);
                _routingManager.HandleSinkSourceChanged(
                    _mapper.Map<AliasIndexModel, Shared.Domain.Models.Media.AliasIndexModel>(evt.Sink),
                    _mapper.Map<AliasIndexModel, Shared.Domain.Models.Media.AliasIndexModel>(evt.Source));
            });

            AddSubscription<DiskSpaceEvent>(evt => _hubContext.Clients.All.OnDiskSpaceStateChanged(evt));

            AddDataSubscription<ActiveProcedureState>(data =>
            {
                var mapped = _mapper.Map<Avalanche.Api.ViewModels.ActiveProcedureViewModel>(data);
                _hubContext.Clients.All.OnActiveProcedureStateChanged(mapped);
            });

            AddDataSubscription<PgsDisplayStateData>(data => _hubContext.Clients.All.OnPgsDisplayStateDataChanged(data));
            AddDataSubscription<PgsTimeoutPlayerData>(data => _hubContext.Clients.All.OnPgsTimeoutPlayerDataChanged(data));
            AddDataSubscription<DisplayRecordStateData>(data => _hubContext.Clients.All.OnDisplayBasedRecordingStateDataChanged(data));

            AddSubscription<RecorderStateEvent>(evt => _hubContext.Clients.All.OnRecorderStateChanged(evt));
            AddSubscription<TimeoutStateData>(evt => _hubContext.Clients.All.OnTimeoutStateDataChanged(evt));
            AddSubscription<SystemErrorRaisedEvent>(evt => _hubContext.Clients.All.OnSystemErrorRaised(evt));
            AddSubscription<SystemPersistantNotificationRaisedEvent>(evt => _hubContext.Clients.All.OnSystemSystemPersistantNotificationRaised(evt));

            AddSubscription<PgsTimeoutRoomStateEvent>(evt => _hubContext.Clients.All.OnPgsTimeoutRoomStateChanged(evt));

            AddSubscription<ImageCaptureStartedEvent>(evt => _hubContext.Clients.All.OnImageCaptureStarted(evt));

            AddDataSubscription<SelectedSourceStateData>(data => _hubContext.Clients.All.OnSelectedSourceStateDataChanged(data));

            AddSubscription<ServiceSessionCreatedEvent>(evt => _hubContext.Clients.All.OnServiceSessionStarted(evt));

            AddSubscription<ServiceSessionEndedEvent>(evt => _hubContext.Clients.All.OnServiceSessionEnded(evt));

            AddSubscription<ServiceSessionAttendeesChanged>(evt => _hubContext.Clients.All.OnServiceSessionAttendeesChanged(evt));


            return Task.CompletedTask;
        }

        private void AddSubscription<TEvent>(Action<TEvent> handler)
            where TEvent : StateEvent
        {
            var id = _stateClient.SubscribeEvent<TEvent>(handler);
            _subscriptions.Add(id);
        }

        private void AddDataSubscription<TData>(Action<TData> handler)
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
