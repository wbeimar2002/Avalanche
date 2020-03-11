using Ism.Api.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Api.Broadcaster.EventArgs
{
    public class BroadcastEventArgs : System.EventArgs
    {
        public MessageRequest MessageRequest { get; private set; }

        public BroadcastEventArgs(MessageRequest messageRequest)
        {
            MessageRequest = messageRequest ?? new MessageRequest();
        }
    }
}
