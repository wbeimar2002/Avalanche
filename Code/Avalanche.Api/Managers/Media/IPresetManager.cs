using Avalanche.Shared.Domain.Models.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPresetManager
    {
        Task ApplyPreset(RoutingPresetModel presetViewModel);
        Task SavePreset(string physician, RoutingPresetModel presetViewModel);
        Task<List<RoutingPresetModel>> GetPresets(string physician);
    }
}
