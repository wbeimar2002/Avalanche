using Avalanche.Api.Broadcaster.EventArgs;
using Avalanche.Api.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Broadcaster.Services
{
    public class BroadcastService : IBroadcastService
    {
        #region Public Variables 

        private EventHandler<BroadcastEventArgs> messageListenedHandler;
        private readonly object eventLocker = new object();

        #endregion

        #region Public Methods 

        public void Broadcast(MessageRequest messageRequest)
        {
            EventHandler<BroadcastEventArgs> handler;
            lock (eventLocker)
            {
                handler = messageListenedHandler;
                if (handler != null)
                {
                    handler(this, new BroadcastEventArgs(messageRequest));
                }
            }
        }

        #endregion

        #region Public Event 

        public event EventHandler<BroadcastEventArgs> MessageListened
        {
            add
            {
                lock (eventLocker)
                {
                    messageListenedHandler += value;
                }
            }
            remove
            {
                lock (eventLocker)
                {
                    messageListenedHandler -= value;
                }
            }
        }

        #endregion
    }
}
