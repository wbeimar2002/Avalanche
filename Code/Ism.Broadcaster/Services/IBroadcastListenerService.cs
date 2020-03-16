using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.Services
{
    public interface IBroadcastListenerService
    {
        string HubURL { get; set; }
        EventNameEnum HubEventName { get; set; }
        bool IsConnected { get; }
        BroadcastEventArgs BroadcastEventArgs { get; }
    }
}
