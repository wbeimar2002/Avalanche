using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Models
{
    public class MessageRequest
    {
        public string Content { get; set; }
        public int EventGroup { get; set; }
    }
}
