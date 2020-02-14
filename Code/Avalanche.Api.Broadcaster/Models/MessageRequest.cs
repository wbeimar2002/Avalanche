using Avalanche.Api.Broadcaster.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Broadcaster.Models
{
    public class MessageRequest
    {
        public string Message { get; set; }
        public EventNameEnum EventName { get; set; }
    }
}
