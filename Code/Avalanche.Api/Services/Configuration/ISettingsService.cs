using Avalanche.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public interface ISettingsService
    {
        Task<TimeoutSettings> GetTimeoutSettingsAsync();
        Task<PatientsSetupSettings> GetPatientsSetupSettingsAsync();
    }
}
