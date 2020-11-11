using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class CommandResponse
    {
        public CommandResponse(Device device)
        {
            this.Device = device;
        }

        public Device Device { get; private set; }
        public List<string> Messages { get; set; }
    }
}
