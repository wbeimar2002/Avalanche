using Avalanche.Shared.Domain.Models.Media;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPresetManager
    {
        Task ApplyPreset(RoutingPresetModel presetViewModel);
    }
}
