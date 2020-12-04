using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(Avalanche.Shared.Domain.Models.User user, SectionViewModel category);
        Task<SectionViewModel> GetCategoryByKey(Avalanche.Shared.Domain.Models.User user, string key);
        Task<SectionReadOnlyViewModel> GetCategoryByKeyReadOnly(Avalanche.Shared.Domain.Models.User user, string key);

        Task<TimeoutSettings> GetTimeoutSettings(Avalanche.Shared.Domain.Models.User user);
        Task<SetupSettings> GetSetupSettings(Avalanche.Shared.Domain.Models.User user);
        Task<RoutingSettings> GetRoutingSettings(Avalanche.Shared.Domain.Models.User user);
    }
}
