using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class CommandResponse
    {
        public CommandResponse(VideoDevice device)
        {
            this.Device = device;
        }

        public VideoDevice Device { get; private set; }
        public List<string> Messages { get; set; }
    }
}
