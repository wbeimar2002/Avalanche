using AutoMapper;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public class MaintenanceManager : IMaintenanceManager
    {
        readonly IStorageService _storageService;
        readonly IMetadataManager _metadataManager;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        private List<KeyValuePairViewModel> _types;

        public MaintenanceManager(IStorageService storageService, ISettingsService settingsService, IMetadataManager metadataManager, IMapper mapper)
        {
            _storageService = storageService;
            _settingsService = settingsService;
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

            await _storageService.SaveJson(category.JsonKey, JsonConvert.SerializeObject(category), 1, configurationContext);
        }

        public async Task<SectionReadOnlyViewModel> GetCategoryByKeyReadOnly(Avalanche.Shared.Domain.Models.User user, string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            configurationContext.IdnId = new Guid().ToString();
            return await _storageService.GetJson<SectionReadOnlyViewModel>(key, 1, configurationContext);
        }

        public async Task<SectionViewModel> GetCategoryByKey(Shared.Domain.Models.User user, string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            var category = await _storageService.GetJson<SectionViewModel>(key, 1, configurationContext);

            if (_types == null)
                _types = await _metadataManager.GetMetadata(user, MetadataTypes.SettingTypes);

            await SetSources(configurationContext, category);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SetSources(configurationContext, section);
                }
            }

            category.JsonKey = key;
            return category;
        }

        private async Task SetSources(ConfigurationContext configurationContext, SectionViewModel category)
        {
            if (category!= null && category.Settings != null)
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
        public async Task<T> GetSettings<T>(string key, User user)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);

            switch (key)
            {
                case "PgsSettings":
                    return ChangeType<T>(await _settingsService.GetPgsSettings(configurationContext));
                case "TimeoutSettings":
                    return ChangeType<T>(await _settingsService.GetTimeoutSettings(configurationContext));
                case "RoutingSettings":
                    return ChangeType<T>(await _settingsService.GetRoutingSettings(configurationContext));
                case "SetupSettings":
                    return ChangeType<T>(await _settingsService.GetSetupSettings(configurationContext));
                default:
                    return default(T);
            }
        }

        private T ChangeType<T>(object o)
        {
            Type conversionType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(o, conversionType);
        }

        private async Task SaveAndCleanSources(ConfigurationContext configurationContext, SectionViewModel category)
        {
            if (category.Settings != null)
            {
                foreach (var item in category.Settings)
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
