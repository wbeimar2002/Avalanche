using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Command
    {
        public Source Source { get; set; }
        public List<Output> Outputs { get; set; }
        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
