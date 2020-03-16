using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.Services
{
    public class BroadcastService : IBroadcastService
    {
        #region Public Variables 

        private EventHandler<BroadcastEventArgs> messageListenedHandler;
        private readonly object eventLocker = new object();

        #endregion

        #region Public Methods 

        public void Broadcast(MessageRequest messageRequest, Action<MessageRequest> externalAction = null)
        {
            EventHandler<BroadcastEventArgs> handler;
            lock (eventLocker)
            {
                handler = messageListenedHandler;
                if (handler != null)
                {
                    handler(this, new BroadcastEventArgs(messageRequest, externalAction));
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
