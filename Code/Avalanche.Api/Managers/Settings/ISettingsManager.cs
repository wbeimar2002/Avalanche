using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public interface ISettingsManager
    {
        Task<List<SettingCategory>> GetCategories();
        Task<SettingCategoryViewModel> GetSettingsByCategory(string categoryKey);
        List<KeyValuePairViewModel> GetSourceValuesByCategory(string categoryKey, string sourceKey);

        Task SaveSettingsByCategory(string categoryKey, List<KeyValuePairViewModel> settings);
    }
}
