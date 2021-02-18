using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class RoutingPreviewViewModel : RoutingActionViewModel
    {
        public int Index { get; set; }
        public RegionModel Region { get; set; }
    }
}
