using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Ism.Api.Broadcaster.Enumerations
{
    public enum EventNameEnum
    {
        Unknown = 0,

        [Description("OnException")]
        OnException = 1
    }
}
