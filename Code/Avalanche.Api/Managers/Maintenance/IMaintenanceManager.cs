using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(DynamicSectionViewModel category);
        Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action);
        Task<DynamicSectionViewModel> GetCategoryByKey(string key);
        Task<DynamicListViewModel> GetCategoryListByKey(string key, string parentId);
        Task<List<dynamic>> GetListValues(string key);
        Task<dynamic> GetSettingValues(string key);
        Task SaveCategoryPolicies(DynamicSectionViewModel category);
        Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest);
        Task<GeneralApiConfiguration> GetGeneralApiConfigurationSettings();
    }
}
