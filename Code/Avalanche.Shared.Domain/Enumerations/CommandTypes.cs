using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    public enum CommandTypes
    {
        Play,
        Stop,
        Mute,
        GetVolumeUp,
        GetVolumeDown,
        HandleMessage,
    }
}
