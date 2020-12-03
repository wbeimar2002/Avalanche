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

        public async Task<TimeoutSettings> GetTimeoutSettings()
        {
            return await _settingsService.GetTimeoutSettings();
        }
    }
}
