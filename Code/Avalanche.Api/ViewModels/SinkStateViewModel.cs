using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class SinkStateViewModel 
    {
        public SinkModel Sink { get; set; }
        public string Value { get; set; }
    }
}
