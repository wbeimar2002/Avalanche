using Ism.Broadcaster.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.Models
{
    public class MessageRequest
    {
        public string Topic { get; set; }
        public string Content { get; set; }
        public EventNameEnum EventName { get; set; }
    }
}
