using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class RoutesViewModel
    {
        public List<VideoDeviceModel> Sources { get; set; }
        public List<VideoDeviceModel> Destinations { get; set; }
    }
}
