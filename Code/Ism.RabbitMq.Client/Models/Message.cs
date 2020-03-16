using Ism.RabbitMq.Client.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Models
{
    public class Message
    {
        public string Content { get; set; }
        public EventNameEnum EventName { get; set; }
    }
}
