﻿using Avalanche.Shared.Infrastructure.Models;
using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public interface ISettingsService
    {
        Task<TimeoutSettings> GetTimeoutSettingsAsync();
        Task<SetupSettings> GetSetupSettingsAsync(ConfigurationContext context);
        Task<RoutingSettings> GetVideoRoutingSettingsAsync(ConfigurationContext context);
    }
}
