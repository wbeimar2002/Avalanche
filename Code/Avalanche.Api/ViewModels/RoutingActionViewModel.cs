using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class RoutingActionViewModel
    {
        public int UserInterfaceId { get; set; }
        public SinkModel Sink { get; set; }
    }
}
