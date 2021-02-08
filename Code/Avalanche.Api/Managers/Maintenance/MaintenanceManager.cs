using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;
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
        readonly IHttpContextAccessor _httpContextAccessor;

        readonly User user;
        readonly ConfigurationContext configurationContext;

        public MaintenanceManager(IStorageService storageService, 
            IMetadataManager metadataManager, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor)
        {
            _storageService = storageService;
            _metadataManager = metadataManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task SaveCategoryPolicies(DynamicSectionViewModel category)
        {
            await SaveJsonValues(category, configurationContext);

            SettingsHelper.CleanSettings(category);

            await _storageService.SaveJson(category.JsonKey, JsonConvert.SerializeObject(category), 1, configurationContext);
        }

        public async Task SaveCategory(DynamicSectionViewModel category)
        {
            await SaveSources(category);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SaveSources(section);
                }
            }

            await SaveJsonValues(category, configurationContext);
        }

        public async Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action)
        {

            if (category.SaveAsFile)
            {
                await SaveCustomListFile(category);
            }
            else
                await SaveCustomEntity(user, category, action);
        }

        private async Task SaveCustomListFile(DynamicListViewModel category)
        {
            var result = JsonConvert.SerializeObject(new { Items = category.Data });

            if (await SchemaIsValid(category.Schema, result, configurationContext))
            {
                await _storageService.SaveJson(category.SourceKey, result, 1, configurationContext);
            }
            else
            {   //TODO: Pending Exceptions strategy
                throw new ValidationException("Json Schema Invalid for " + category.SourceKey + " values");
            }
        }

        public async Task<DynamicSectionViewModel> GetCategoryByKey(string key)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<DynamicSectionViewModel>(key, 1, configurationContext);
            var settingValues = await _storageService.GetJsonDynamic(key + "Values", 1, configurationContext);

            var types = await _metadataManager.GetMetadata(MetadataTypes.SettingTypes);
            var policiesTypes = (await _storageService.GetJsonObject<ListContainerViewModel>("SettingsPolicies", 1, configurationContext)).Items;           

            await SetSources(category, types);
            SettingsHelper.SetSettingValues(category, settingValues, policiesTypes);

            if (category.Sections != null)
            {
                await SetSettingsValues(category, settingValues, types, policiesTypes);
            }

            category.JsonKey = key;
            return category;
        }

        public async Task<DynamicListViewModel> GetCategoryListByKey(string key)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, configurationContext);
            if (category.SaveAsFile)
            {
                var values = await _storageService.GetJsonObject<DynamicListContainerViewModel>(category.SourceKey, 1, configurationContext);
                category.Data = values.Items;

                foreach (var item in category.Properties)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = await GetDynamicSourceValues(item.SourceKey);
                    }
                }
            }
            else
            {
                category.Data = await GetData(category.SourceKey);

                foreach (var item in category.Properties)
                {
                    SetIsRequired(configurationContext, category.SourceKey, item);

                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = await GetDynamicSourceValues(item.SourceKey);

                        foreach (var element in category.Data)
                        {
                            var serializedElement = JsonConvert.SerializeObject(element);
                            JObject jsonElement = JObject.Parse(serializedElement);
                            string relatedObject = JsonConvert.SerializeObject(item.SourceValues.Where(s => s.Id == jsonElement[item.JsonKey].ToString()).FirstOrDefault()?.RelatedObject);

                            jsonElement.Add(new JProperty(item.JsonKeyForRelatedObject, JObject.Parse(relatedObject)));

                            dynamic jsonElementObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonElement.ToString());

                            var _original = (IDictionary<string, object>)jsonElementObject;
                            var _clone = (IDictionary<string, object>)element;

                            foreach (var propertyKey in _original)
                                _clone[propertyKey.Key] = propertyKey.Value;

                        }
                    }
                }
            }

            return category;
        }

        private async Task SetIsRequired(ConfigurationContext configurationContext, string key, DynamicPropertyViewModel item)
        {
            //It is a switch because this will grow
            switch (key)
            {
                case "DepartmentsMetadata":
                    switch (item.JsonKey)
                    {
                        case "DepartmentId":
                            dynamic setupSettings = await _storageService.GetJsonDynamic("SetupSettingsValues", 1, configurationContext);
                            item.Required = setupSettings.General.DepartmentsSupported;
                            break;
                    }
                    break;
            }
        }

        private async Task<IList<KeyValuePairObjectViewModel>> GetDynamicSourceValues(string sourceKey)
        {
            try
            {
                switch (sourceKey)
                {
                    case "Departments":
                        var departments = await GetData("Departments");
                        return departments.Select(d => new KeyValuePairObjectViewModel()
                        {
                            Id = ((dynamic)d).Id.ToString(),
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
                        return new List<KeyValuePairObjectViewModel>();
                }
            }
            catch (Exception ex)
            {
                return new List<KeyValuePairObjectViewModel>();
            }
        }

        private async Task SetSettingsValues(DynamicSectionViewModel rootSection, dynamic settingValues, IList<KeyValuePairViewModel> types, IList<KeyValuePairViewModel> policiesTypes)
        {
            foreach (var section in rootSection.Sections)
            {
                var sectionValues = settingValues == null ? null : settingValues[section.JsonKey];

                await SetSources(section, types);
                SettingsHelper.SetSettingValues(section, sectionValues, policiesTypes);

                if (section.Sections != null)
                {
                    await SetSettingsValues(section, sectionValues, types, policiesTypes);
                }
            }
        }

        public async Task<JObject> GetSettingValues(string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.User, ConfigurationContext>(user);
            return await _storageService.GetJsonDynamic(key, 1, configurationContext);
        }

        private async Task SetSources(DynamicSectionViewModel category, IList<KeyValuePairViewModel> types)
        {
            if (category!= null && category.Settings != null)
            {
                foreach (var item in category.Settings)
                {
                    if (item.VisualStyle == VisualStyles.CustomList)
                    {
                        item.CustomList = await GetCategoryListByKey(item.SourceKey);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.SourceKey))
                        {
                            item.SourceValues = (await _storageService.GetJsonObject<SourceListContainerViewModel>(item.SourceKey, 1, configurationContext)).Items;
                            item.SourceValues.ToList().ForEach(s => s.Types = types);
                        }
                    }
                }
            }
        }

        private async Task SaveSources(DynamicSectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var item in section.Settings)
                {
                    if (string.IsNullOrEmpty(item.JsonKey) && !string.IsNullOrEmpty(item.SourceKey))
                    {
                        if (item.VisualStyle == VisualStyles.CustomList)
                        {
                            var category = item.CustomList;

                            if (category.SaveAsFile)
                                SaveCustomListFile(category);
                            else
                                await SaveCustomEntities(user, category);
                        }
                        else
                        {
                            await _storageService.SaveJson(item.SourceKey, JsonConvert.SerializeObject(new { Items = item.SourceValues }), 1, configurationContext);
                            item.SourceValues = null;
                        }
                    }
                }
            }
        }

        private async Task SaveJsonValues(DynamicSectionViewModel category, ConfigurationContext configurationContext)
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
            if (string.IsNullOrEmpty(schemaKey))
                return true;
            else
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
        }

        private async Task SaveCustomEntity(User user, DynamicListViewModel category, DynamicListActions action)
        {
            switch (category.SourceKey)
            {
                case "Departments":
                    await SaveDepartments(action, category.Entity);
                    break;
                case "ProcedureTypes":
                    await SaveProcedureTypes(action, category.Entity);
                    break;
                default:
                    //TODO: Pending Exceptions strategy
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveCustomEntities(User user, DynamicListViewModel category)
        {
            switch (category.SourceKey)
            {
                case "Departments":
                    foreach (var item in category.NewData)
                    {
                        await SaveDepartments(DynamicListActions.Insert, item);
                    }
                    break;
                case "ProcedureTypes":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveDepartments(DynamicListActions.Delete, item);
                    }
                    break;
            }
        }

        private async Task SaveProcedureTypes(DynamicListActions action, dynamic source)
        {
            var procedureType = new ProcedureType();
            procedureType.DepartmentId = Convert.ToInt32(source.Department?.Id);

            Helpers.Mapper.Map(source, procedureType);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _metadataManager.AddProcedureType(procedureType);
                    break;
                case DynamicListActions.Delete:
                    await _metadataManager.DeleteProcedureType(procedureType);
                    break;
                default:
                    //TODO: Pending Exceptions strategy
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveDepartments(DynamicListActions action, dynamic source)
        {
            var department = new Department();
            Helpers.Mapper.Map(source, department);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _metadataManager.AddDepartment(department);
                    break;
                case DynamicListActions.Delete:
                    await _metadataManager.DeleteDepartment(department.Id);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task<List<ExpandoObject>> GetData(string sourceKey)
        {
            switch (sourceKey)
            {
                case "Departments":
                    var departments = await _metadataManager.GetAllDepartments();

                    return  departments
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.Id = item.Id;
                            expandoObj.Name = item.Name;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();
                case "ProcedureTypes":

                    var procedureTypes = await _metadataManager.GetProcedureTypesByDepartment(1);

                    return procedureTypes
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.Id = item.Id;
                            expandoObj.Name = item.Name;
                            expandoObj.DepartmentId = item.DepartmentId;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();
                default:
                    return new List<ExpandoObject>();
            }
        }
    }
}
