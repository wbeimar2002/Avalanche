using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalanche.Shared.Domain.Models
{
    public class Device
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; } //TODO: Confirm with Gabe
    }
}
