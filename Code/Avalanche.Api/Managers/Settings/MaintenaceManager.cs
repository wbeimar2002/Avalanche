using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Ism.Common.Core.Configuration.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public class MaintenaceManager : IMaintenaceManager
    {
        readonly IStorageService _storageService;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        public MaintenaceManager(IStorageService storageService, ISettingsService settingsService, IMapper mapper)
        {
            _storageService = storageService;
            _settingsService = settingsService;
            _mapper = mapper;
        }

        public async Task SaveCategory(Avalanche.Shared.Domain.Models.User user, SectionViewModel category)
        {
            //TODO: Save to storage
            await Task.CompletedTask;
        }

        public async Task<SectionViewModel> GetCategoryByKey(Avalanche.Shared.Domain.Models.User user, string key)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _storageService.GetJson<SectionViewModel>(key, 1, configurationContext);
        }
    }
}
