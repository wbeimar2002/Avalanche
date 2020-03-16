using Ism.Broadcaster.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.Broadcaster.Models
{
    public class MessageRequest
    {
        public string Content { get; set; }
        public EventGroupEnum EventGroup { get; set; }
    }
}
