using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Presets
{
    public class UserPresetsModel
    {
        public Dictionary<int, RoutingPresetModel> RoutingPresets { get; set; } = new Dictionary<int, RoutingPresetModel>();
    }
}
