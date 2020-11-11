using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Source : Device
    {
        public Output Output { get; set; }
        public bool IsDynamic { get; set; }
    }
}
