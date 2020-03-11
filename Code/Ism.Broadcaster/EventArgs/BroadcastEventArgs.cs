using Ism.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.EventArgs
{
    public class BroadcastEventArgs : System.EventArgs
    {
        public MessageRequest MessageRequest { get; private set; }
        public Action<MessageRequest> ExternalAction { get; private set; }

        public BroadcastEventArgs(MessageRequest messageRequest, Action<MessageRequest> externalAction = null)
        {
            ExternalAction = externalAction;
            MessageRequest = messageRequest ?? new MessageRequest();
        }
    }
}
