using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.ViewModels
{
    public class CommandResponseViewModel
    {
        public string OutputId { get; set; }
        public List<string> Messages { get; set; }
        public string SessionId { get; internal set; }
        public int ResponseCode { get; internal set; }
    }
}
