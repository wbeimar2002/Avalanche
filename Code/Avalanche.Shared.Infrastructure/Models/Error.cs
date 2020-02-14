using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class Error
    {
        public int Code { get; set; }
        public string RequestUrl { get; set; }
        public string ReasonPhrase { get; set; }
        public string Description { get; set; }
        public string StackTrace { get; set; }
        public int Result { get; set; }
    }
}
