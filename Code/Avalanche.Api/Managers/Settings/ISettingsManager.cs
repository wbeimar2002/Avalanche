using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public interface ISettingsManager
    {
        Task<TimeoutSettings> GetTimeoutSettings();      
    }
}
