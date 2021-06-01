using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Presets
{
    public class PresetsModel
    {
        public Dictionary<string, UserPresetsModel> Users { get; set; } = new Dictionary<string, UserPresetsModel>();
    }
}
