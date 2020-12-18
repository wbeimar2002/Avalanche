﻿using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(User user, SectionViewModel category);
        Task<SectionViewModel> GetCategoryByKey(User user, string key);
        Task<SectionReadOnlyViewModel> GetCategoryByKeyReadOnly(User user, string key);

        Task<T> GetSettings<T>(string key, User user);
    }
}