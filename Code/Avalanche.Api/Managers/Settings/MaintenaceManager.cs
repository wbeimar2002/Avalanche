using AutoMapper;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Ism.Common.Core.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public class MaintenaceManager : IMaintenaceManager
    {
        readonly IStorageService _storageService;
        readonly IMetadataManager _metadataManager;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        private List<KeyValuePairViewModel> _types;

        public MaintenaceManager(IStorageService storageService, ISettingsService settingsService, IMetadataManager metadataManager, IMapper mapper)
        {
            _storageService = storageService;
            _settingsService = settingsService;
            _metadataManager = metadataManager;
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
            var category = await _storageService.GetJson<SectionViewModel>(key, 1, configurationContext);

            if (_types == null)
                _types = await _metadataManager.GetMetadata(user, Shared.Domain.Enumerations.MetadataTypes.SettingTypes);

            await SetSources(configurationContext, category);

            foreach (var section in category.Sections)
            {
                await SetSources(configurationContext, section);
            }

            return category;
        }

        private async Task SetSources(ConfigurationContext configurationContext, SectionViewModel category)
        {
            if (category.Settings != null)
            {
                foreach (var item in category.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = (await _storageService.GetJson<SourceListContainerViewModel>(item.SourceKey, 1, configurationContext)).Items;
                        
                        if (_types != null)
                            item.SourceValues.ForEach(s => s.Types = _types);
                    }
                }
            }
        }
    }
}
