using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.Services
{
    public interface IBroadcastService
    {
        void Broadcast(MessageRequest messageRequest);
        event EventHandler<BroadcastEventArgs> MessageListened;
    }
}
