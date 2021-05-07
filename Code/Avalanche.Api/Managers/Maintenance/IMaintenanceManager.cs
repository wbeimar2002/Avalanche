using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        IEnumerable<FileSystemElementViewModel> GetFiles(string folder, string filter);
        IEnumerable<FileSystemElementViewModel> GetFolders(string folder);

        Task SaveCategory(DynamicSectionViewModel category);
        Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action);
        Task<DynamicSectionViewModel> GetCategoryByKey(string key);
        Task<DynamicListViewModel> GetCategoryListByKey(string key);
        Task<JObject> GetSettingValues(string key);
        Task SaveCategoryPolicies(DynamicSectionViewModel category);
        Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest);
    }
}
