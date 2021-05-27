using Avalanche.Shared.Domain.Models.Presets;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Presets
{
    public interface IPresetManager
    {
        Task ApplyPreset(int index);
        Task SavePreset(int index, string name);
        Task<UserPresetsModel> GetPresets();
    }
}
