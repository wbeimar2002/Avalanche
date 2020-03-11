using Ism.Api.Broadcaster.EventArgs;
using Ism.Api.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Api.Broadcaster.Services
{
    public interface IBroadcastService
    {
        void Broadcast(MessageRequest messageRequest);
        event EventHandler<BroadcastEventArgs> MessageListened;
    }
}
