using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Models
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; }
        public string QueueName { get; set; }
        public int Port { get; set; }
        public int ManagementPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
