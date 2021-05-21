using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class RoutingPresetModel
    {
        public List<RouteModel> Routes { get; set; } = new List<RouteModel>();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
