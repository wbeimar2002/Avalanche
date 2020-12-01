using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Ism.Common.Core.Configuration.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    [ExcludeFromCodeCoverage]
    public class SettingsManager : ISettingsManager
    {
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        public SettingsManager(ISettingsService settingsService, IMapper mapper)
        {
            _settingsService = settingsService;
            _mapper = mapper;
        }

        public async Task<RoutingSettings> GetVideoRoutingSettingsAsync(Avalanche.Shared.Domain.Models.User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _settingsService.GetVideoRoutingSettingsAsync(configurationContext);
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            return await _settingsService.GetTimeoutSettingsAsync();
        }

        public async Task<SetupSettings> GetSetupSettingsAsync(Avalanche.Shared.Domain.Models.User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _settingsService.GetSetupSettingsAsync(configurationContext);
        }
    }
}
