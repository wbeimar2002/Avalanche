using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public class MaintenanceManager : IMaintenanceManager
    {
        readonly IStorageService _storageService;
        readonly IMetadataManager _metadataManager;
        readonly IMapper _mapper;

        private List<KeyValuePairViewModel> _types;

        public MaintenanceManager(IStorageService storageService, IMetadataManager metadataManager, IMapper mapper)
        {
            _storageService = storageService;
            _metadataManager = metadataManager;
            _mapper = mapper;
        }

        public async Task SaveCategory(Avalanche.Shared.Domain.Models.User user, SectionViewModel category)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            configurationContext.IdnId = new Guid().ToString();

            await SaveAndCleanSources(configurationContext, category);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SaveAndCleanSources(configurationContext, section);
                }
            }

            string result = SettingsHelper.GetJsonValues(category);
            await _storageService.SaveJson(category.JsonKey + "Values", result, 1, configurationContext);
        }

        public async Task<SectionViewModel> GetCategoryByKey(User user, string key)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<SectionViewModel>(key, 1, configurationContext);
            var settingValues = await _storageService.GetJsonDynamic(key + "Values", 1, configurationContext);

            _types = await _metadataManager.GetMetadata(user, MetadataTypes.SettingTypes);

            await SetSources(configurationContext, category);
            SettingsHelper.SetSettingValues(category, settingValues);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    var sectionValues = settingValues == null ? null : settingValues[section.JsonKey];

                    await SetSources(configurationContext, section);
                    SettingsHelper.SetSettingValues(section, sectionValues);
                }
            }

            category.JsonKey = key;
            return category;
        }

        public async Task<JObject> GetSettingValues(string key, User user)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _storageService.GetJsonDynamic(key, 1, configurationContext);
        }

        private async Task SetSources(ConfigurationContext configurationContext, SectionViewModel category)
        {
            if (category!= null && category.Settings != null)
            {
                foreach (var item in category.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = (await _storageService.GetJsonObject<SourceListContainerViewModel>(item.SourceKey, 1, configurationContext)).Items;
                        
                        if (_types != null)
                            item.SourceValues.ForEach(s => s.Types = _types);
                    }
                }
            }
        }

        private async Task SaveAndCleanSources(ConfigurationContext configurationContext, SectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var item in section.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues.ForEach(s => s.Types = null);

                        await _storageService.SaveJson(item.SourceKey, JsonConvert.SerializeObject(new { Items = item.SourceValues }), 1, configurationContext);

                        item.SourceValues = null;
                    }
                }
            }
        }
    }
}
