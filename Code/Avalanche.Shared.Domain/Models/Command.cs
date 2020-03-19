using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Command
    {
        public CommandTypes CommandType { get; set; }
        public string Value { get; set; }
    }
}
