using Avalanche.Api.Broadcaster.Enumerations;
using Avalanche.Api.Broadcaster.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Broadcaster.Services
{
    public interface IBroadcastListenerService
    {
        string HubURL { get; set; }
        EventNameEnum HubEventName { get; set; }
        bool IsConnected { get; }
        BroadcastEventArgs BroadcastEventArgs { get; }
    }
}
