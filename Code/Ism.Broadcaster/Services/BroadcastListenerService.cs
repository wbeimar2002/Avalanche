using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Extensions;
using Ism.Broadcaster.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Broadcaster.Services
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
        public EventGroupEnum HubEventName { get; set; }
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

        public BroadcastListenerService(string hubURL, EventGroupEnum hubEventName)
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

            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                hubConnection.StartAsync().
                    ContinueWith(task
                        =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("Signal R connection faulted.");
                            throw task.Exception;
                        }
                        else
                        {
                            Console.WriteLine("Signal R connection open.");

                            // Register/attach broadcast listener event
                            if (HubEventName != EventGroupEnum.Unknown)
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
                                Content = message,
                                EventGroup = HubEventName
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