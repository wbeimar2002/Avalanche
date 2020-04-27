using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class CommandResponse
    {
        public string OutputId { get; set; }
        public List<string> Messages { get; set; }
    }
}
