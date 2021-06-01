using Avalanche.Shared.Domain.Models.Presets;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Presets
{
    public interface IPresetManager
    {
        Task ApplyPreset(string userId, int index);
        Task RemovePreset(string userId, int index);
        Task SavePreset(string userId, int index, string name);
        Task<UserPresetsModel> GetPresets(string userId);
    }
}
