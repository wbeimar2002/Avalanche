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
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Maintenance
{
    public class MaintenanceManager : IMaintenanceManager
    {
        readonly IStorageService _storageService;
        readonly IMetadataManager _metadataManager;
        readonly IMapper _mapper;

        public MaintenanceManager(IStorageService storageService, IMetadataManager metadataManager, IMapper mapper)
        {
            _storageService = storageService;
            _metadataManager = metadataManager;
            _mapper = mapper;
        }

        public async Task SaveCategoryPolicies(User user, SectionViewModel category)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();

            await SaveJsonValues(category, configurationContext);

            await _storageService.SaveJson(category.JsonKey, JsonConvert.SerializeObject(category), 1, configurationContext);
        }

        public async Task SaveCategory(User user, SectionViewModel category)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();

            await SaveSources(configurationContext, category);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SaveSources(configurationContext, section);
                }
            }

            await SaveJsonValues(category, configurationContext);
        }

        public async Task SaveEntityChanges(User user, DynamicListViewModel category)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();

            var result = JsonConvert.SerializeObject(new { Items = category.Data });

            if (category.SaveAsFile)
            {
                if (await SchemaIsValid(category.Schema, result, configurationContext))
                {
                    await _storageService.SaveJson(category.Schema, result, 1, configurationContext);
                }
                else
                {   //TODO: Pending Exceptions strategy
                    throw new ValidationException("Json Schema Invalid for " + category.SourceKey + " values");
                }
            }
            else
                await SaveDestination(user, category);
        }

        public async Task<SectionViewModel> GetCategoryByKey(User user, string key)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<SectionViewModel>(key, 1, configurationContext);
            var settingValues = await _storageService.GetJsonDynamic(key + "Values", 1, configurationContext);

            var types = await _metadataManager.GetMetadata(user, MetadataTypes.SettingTypes);
            var policiesTypes = (await _storageService.GetJsonObject<ListContainerViewModel>("SettingsPolicies", 1, configurationContext)).Items;           

            await SetSources(configurationContext, category, types);
            SettingsHelper.SetSettingValues(category, settingValues, policiesTypes);

            if (category.Sections != null)
            {
                await SetSettingsValues(configurationContext, category, settingValues, types, policiesTypes);
            }

            category.JsonKey = key;
            return category;
        }

        public async Task<DynamicListViewModel> GetCategoryListByKey(User user, string key)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, configurationContext);

            if (category.SaveAsFile)
            {
                var values = await _storageService.GetJsonObject<DynamicListContainerViewModel>(category.SourceKey, 1, configurationContext);
                category.Data = values.Items;

                foreach (var item in category.Properties)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = await GetDynamicSourceValues(item.SourceKey, configurationContext, user);
                    }
                }
            }
            else
            {
                category.Data = await GetData(user, category.SourceKey);
            }

            return category;
        }

        private async Task<IList<KeyValuePairObjectViewModel>> GetDynamicSourceValues(string sourceKey, ConfigurationContext configurationContext, User user)
        {
            switch (sourceKey)
            {
                case "Departments":
                    var departments = await GetData(user, "Departments");
                    return departments.Select(d => new KeyValuePairObjectViewModel()
                    {
                        Id = ((dynamic)d).Id,
                        Value = ((dynamic)d).Name,
                        RelatedObject = d
                    }).ToList();
                case "SinksData":
                    var sinks = (await _storageService.GetJsonObject<DynamicListContainerViewModel>(sourceKey, 1, configurationContext)).Items;
                    return sinks.Select(s => new KeyValuePairObjectViewModel()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = $"{((dynamic)s).Alias} ({((dynamic)s).Index})",
                        RelatedObject = s
                    }).ToList();
                default:
                    return null;
            }
        }

        private async Task SetSettingsValues(ConfigurationContext configurationContext, SectionViewModel rootSection, dynamic settingValues, IList<KeyValuePairViewModel> types, IList<KeyValuePairViewModel> policiesTypes)
        {
            foreach (var section in rootSection.Sections)
            {
                var sectionValues = settingValues == null ? null : settingValues[section.JsonKey];

                await SetSources(configurationContext, section, types);
                SettingsHelper.SetSettingValues(section, sectionValues, policiesTypes);

                if (section.Sections != null)
                {
                    await SetSettingsValues(configurationContext, section, sectionValues, types, policiesTypes);
                }
            }
        }

        public async Task<JObject> GetSettingValues(string key, User user)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _storageService.GetJsonDynamic(key, 1, configurationContext);
        }

        private async Task SetSources(ConfigurationContext configurationContext, SectionViewModel category, IList<KeyValuePairViewModel> types)
        {
            if (category!= null && category.Settings != null)
            {
                foreach (var item in category.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = (await _storageService.GetJsonObject<SourceListContainerViewModel>(item.SourceKey, 1, configurationContext)).Items;
                        item.SourceValues.ForEach(s => s.Types = types);
                    }
                }
            }
        }

        private async Task SaveSources(ConfigurationContext configurationContext, SectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var item in section.Settings)
                {
                    if (string.IsNullOrEmpty(item.JsonKey) && !string.IsNullOrEmpty(item.SourceKey))
                    {
                        await _storageService.SaveJson(item.SourceKey, JsonConvert.SerializeObject(new { Items = item.SourceValues }), 1, configurationContext);
                        item.SourceValues = null;
                    }
                }
            }
        }

        private async Task SaveJsonValues(SectionViewModel category, ConfigurationContext configurationContext)
        {
            string result = SettingsHelper.GetJsonValues(category);

            if (await SchemaIsValid(category.Schema, result, configurationContext))
            {
                await _storageService.SaveJson(category.JsonKey + "Values", result, 1, configurationContext);
            }
            else
            {   //TODO: Pending Exceptions strategy
                throw new ValidationException("Json Schema Invalid for " + category.JsonKey + " values");
            }
        }

        private async Task<bool> SchemaIsValid(string schemaKey, string json, ConfigurationContext configurationContext)
        {
            dynamic dynamicSchema = await _storageService.GetJsonDynamic(schemaKey, 1, configurationContext);
            if (dynamicSchema == null)
                return true;
            else
            {
                string schemaJson = JsonConvert.SerializeObject(dynamicSchema);
                JsonSchema schema = JsonSchema.Parse(schemaJson);

                JObject jsonObject = JObject.Parse(json);

                return jsonObject.IsValid(schema);
            }
        }

        private async Task SaveDestination(User user, DynamicListViewModel category)
        {
            switch (category.SourceKey)
            {
                case "Departments":
                    SaveDepartments(user, category.Action, category.Entity);
                    break;
                case "ProcedureTypes":
                    SaveProcedureTypes(user, category.Action, category.Entity);
                    break;
                default:
                    //TODO: Pending Exceptions strategy
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private void SaveProcedureTypes(User user, string action, dynamic source)
        {
            var destination = new ProcedureType();
            Helpers.Mapper.Map(source, destination);

            switch (action)
            {
                case "Insert":
                    _metadataManager.AddProcedureType(user, destination);
                    break;
                case "Delete":
                    _metadataManager.DeleteProcedureType(user, destination);
                    break;
                default:
                    //TODO: Pending Exceptions strategy
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private void SaveDepartments(User user, string action, dynamic source)
        {
            var destination = new Department();
            Helpers.Mapper.Map(source, destination);

            switch (action)
            {
                case "Insert":
                    break;
                case "Delete":
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task<List<ExpandoObject>> GetData(User user, string sourceKey)
        {
            switch (sourceKey)
            {
                case "Departments":
                    var departments = await _metadataManager.GetAllDepartments(user);

                    return  departments
                               .Select(item =>
                                {
                                    dynamic expandoObj = new ExpandoObject();
                                    expandoObj.Id = item.Id;
                                    expandoObj.Name = item.Name;
                                    return (ExpandoObject)expandoObj;
                                })
                                .ToList();
                default:
                    return new List<ExpandoObject>();
            }
        }
    }
}
