using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Models
{
    public class Error
    {
        public int Code { get; internal set; }
        public string StackTrace { get; internal set; }
        public string Description { get; internal set; }
        public string RequestUrl { get; internal set; }
        public string ReasonPhrase { get; internal set; }
    }
}
