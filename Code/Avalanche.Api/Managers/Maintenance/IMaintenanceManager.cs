using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(User user, SectionViewModel category);
        Task SaveEntityChanges(User user, DynamicListViewModel category);
        Task<SectionViewModel> GetCategoryByKey(User user, string key);
        Task<DynamicListViewModel> GetCategoryListByKey(User user, string key);
        Task<JObject> GetSettingValues(string key, User user);
        Task SaveCategoryPolicies(User user, SectionViewModel category);
    }
}
