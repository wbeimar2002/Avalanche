using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public interface IMaintenaceManager
    {
        Task SaveCategory(Avalanche.Shared.Domain.Models.User user, SectionViewModel category);
        Task<SectionViewModel> GetCategoryByKey(Avalanche.Shared.Domain.Models.User user, string key);
        Task<SectionReadOnlyViewModel> GetCategoryByKeyReadOnly(Avalanche.Shared.Domain.Models.User user, string key);
    }
}
