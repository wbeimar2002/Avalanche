using Avalanche.Api.Broadcaster.EventArgs;
using Avalanche.Api.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Broadcaster.Services
{
    public interface IBroadcastService
    {
        void Broadcast(MessageRequest messageRequest);
        event EventHandler<BroadcastEventArgs> MessageListened;
    }
}
