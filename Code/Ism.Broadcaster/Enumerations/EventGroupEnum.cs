using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Ism.Broadcaster.Enumerations
{
    public enum EventGroupEnum
    {
        Unknown = 0,

        [Description("OnException")]
        OnException = 1,

        [Description("OnTesting")]
        OnTesting = 2,
    }
}
