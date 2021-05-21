using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class RoutingPresetViewModel
    {
        public List<RouteViewModel> Routes { get; set; } = new List<RouteViewModel>();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
