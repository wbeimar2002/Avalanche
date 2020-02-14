using Avalanche.Api.Broadcaster.Enumerations;
using Avalanche.Api.Broadcaster.EventArgs;
using Avalanche.Api.Broadcaster.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Broadcaster.Services
{
    public class BroadcastListenerService : IBroadcastListenerService, IDisposable
    {
        #region Private Members and Variables

        private bool isDisposed = false;
        private readonly object eventLocker = new object();

        private HubConnection hubConnection;
        private event EventHandler<BroadcastEventArgs> BroadcastListenerEventHandler;

        private BroadcastEventArgs _broadcastListenerEventArgs;

        #endregion

        #region Public Properties


        public string HubURL { get; set; }
        public EventNameEnum HubEventName { get; set; }
        public bool IsConnected { get { return hubConnection.State == HubConnectionState.Connected; } }

        public BroadcastEventArgs BroadcastEventArgs
        {
            get
            {
                return _broadcastListenerEventArgs;
            }
        }

        #endregion

        #region Constructor

        public BroadcastListenerService(string hubURL, EventNameEnum hubEventName)
        {
            if (string.IsNullOrWhiteSpace(hubURL) || !Uri.IsWellFormedUriString(hubURL, UriKind.Absolute))
            {
                throw new ArgumentNullException("Broadcast service URL is empty or not well formed value !");
            }

            HubURL = hubURL;
            HubEventName = hubEventName;
        }

        #endregion

        #region Public Methods

        public void ListenHubEvent(Action<object, BroadcastEventArgs> hubEvent)
        {
            if (hubEvent == null)
                throw new ArgumentNullException("HubEvent is null !");

            hubConnection = new HubConnectionBuilder()
                .WithUrl(HubURL)
                .Build();

            try
            {
                hubConnection.StartAsync().
                    ContinueWith(task
                        =>
                    {
                        if (task.IsFaulted)
                        {
                            throw task.Exception;
                        }
                        else
                        {
                            // Register/attach broadcast listener event
                            if (HubEventName != EventNameEnum.Unknown)
                            {
                                lock (eventLocker)
                                {
                                    BroadcastListenerEventHandler += (sender, broadcastArgs) =>
                                        hubEvent.Invoke(sender, broadcastArgs);
                                }
                            }
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion).Wait();
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException;
            }

            hubConnection.On<string>(HubEventName.EnumDescription(),
                    message =>
                    {
                        _broadcastListenerEventArgs = new BroadcastEventArgs(
                            new MessageRequest()
                            {
                                Message = message,
                                EventName = HubEventName
                            });

                        OnMessageListened(_broadcastListenerEventArgs);
                    });


            lock (eventLocker)
            {
                // Unregister/detach broadcast listener event
                BroadcastListenerEventHandler -= (sender, broadCastArgs) =>
                    hubEvent.Invoke(sender, broadCastArgs);
            }
        }

        public async Task ListenHubEventAsync(Action<object, BroadcastEventArgs> hubEvent)
        {
            await new TaskFactory().StartNew(
                () =>
                {
                    ListenHubEvent(hubEvent);
                }
            );
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Private Methods

        private void OnMessageListened(BroadcastEventArgs broadCastArgs)
        {
            EventHandler<BroadcastEventArgs> handler = BroadcastListenerEventHandler;
            if (handler != null)
                handler(this, broadCastArgs);
        }

        private async void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (hubConnection != null)
                    {
                        await hubConnection.DisposeAsync();
                    }
                }

                isDisposed = true;
            }
        }

        #endregion

    }
}