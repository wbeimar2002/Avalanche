using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Ism.RabbitMq.Client.Enumerations
{
    public enum EventNameEnum
    {
        Unknown = 0,

        [Description("OnException")]
        OnException = 1,

        [Description("OkTesting")]
        OnTesting = 2,
    }
}
