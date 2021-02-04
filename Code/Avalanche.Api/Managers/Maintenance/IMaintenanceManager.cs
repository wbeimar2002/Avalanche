using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(User user, DynamicSectionViewModel category);
        Task SaveEntityChanges(User user, DynamicListViewModel category, DynamicListActions action);
        Task<DynamicSectionViewModel> GetCategoryByKey(User user, string key);
        Task<DynamicListViewModel> GetCategoryListByKey(User user, string key);
        Task<JObject> GetSettingValues(string key, User user);
        Task SaveCategoryPolicies(User user, DynamicSectionViewModel category);
    }
}
