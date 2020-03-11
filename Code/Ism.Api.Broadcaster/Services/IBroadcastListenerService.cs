using Ism.Api.Broadcaster.Enumerations;
using Ism.Api.Broadcaster.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Api.Broadcaster.Services
{
    public interface IBroadcastListenerService
    {
        string HubURL { get; set; }
        EventNameEnum HubEventName { get; set; }
        bool IsConnected { get; }
        BroadcastEventArgs BroadcastEventArgs { get; }
    }
}
