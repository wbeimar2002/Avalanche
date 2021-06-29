using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration.Models;
using Ism.Utility.Core;
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
        private readonly IStorageService _storageService;
        private readonly IDataManager _dataManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILibraryService _libraryService;
        private readonly IFilesService _filesService;

        private readonly UserModel user;
        private readonly ConfigurationContext configurationContext;

        public MaintenanceManager(IStorageService storageService, 
            IDataManager dataManager, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService)
        {
            _storageService = storageService;
            _dataManager = dataManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _libraryService = libraryService;
            _filesService = filesService;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest)
        {
            Preconditions.ThrowIfNull(nameof(reindexRequest), reindexRequest);
            Preconditions.ThrowIfNullOrEmpty(nameof(reindexRequest.RepositoryName), reindexRequest.RepositoryName);

            var response = await _libraryService.ReindexRepository(reindexRequest.RepositoryName);
            return _mapper.Map<ReindexStatusViewModel>(response);
        }

        public IEnumerable<FileSystemElementViewModel> GetFiles(string folder, string filter)
        {
            var list = _filesService.GetFiles(folder, filter);
            return list.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
        }

        public IEnumerable<FileSystemElementViewModel> GetFolders(string folder)
        {
            var list = _filesService.GetFolders(folder);
            return list.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = true });
        }

        public async Task SaveCategoryPolicies(DynamicSectionViewModel category)
        {
            await SaveJsonValues(category, configurationContext);

            SettingsHelper.CleanSettings(category);

            await _storageService.SaveJsonMetadata(category.JsonKey + "Metadata", JsonConvert.SerializeObject(category), 1, configurationContext);
        }

        public async Task SaveCategory(DynamicSectionViewModel category)
        {
            await SaveSources(category, category.JsonKey);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SaveSources(section, category.JsonKey);
                }
            }

            await SaveJsonValues(category, configurationContext);
        }

        public async Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action)
        {
            if (category.SaveAsFile)
                await SaveCustomListFile(category);
            else
                await SaveCustomEntity(user, category, action);
        }

        private async Task SaveEmbeddedList(string settingsKey, string jsonKey, DynamicListViewModel customList)
        {
            var result = JsonConvert.SerializeObject(customList.Data);

            if (await SchemaIsValid(customList.Schema, result, configurationContext))
            {
                await _storageService.UpdateJsonProperty(settingsKey, jsonKey, result, 1, configurationContext, true);
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        private async Task SaveCustomListFile(DynamicListViewModel customList)
        {
            var result = JsonConvert.SerializeObject(customList.Data);

            if (await SchemaIsValid(customList.Schema, result, configurationContext))
            {
                await _storageService.SaveJsonObject(customList.SourceKey, result, 1, configurationContext, true);
            }
            else
            {   
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        public async Task<DynamicSectionViewModel> GetCategoryByKey(string key)
        {
            var configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<DynamicSectionViewModel>(key, 1, configurationContext);
            var settingValues = await _storageService.GetJson(category.JsonKey, 1, configurationContext);

            var policiesTypes = (await _storageService.GetJsonObject<List<KeyValuePairViewModel>>("SettingsPolicies", 1, configurationContext));
            var types = await _dataManager.GetList("SettingTypes");

            await SetCategorySources(category, settingValues, types);

            SettingsHelper.SetSettingValues(category, settingValues, policiesTypes);

            if (category.Sections != null)
            {
                await SetSettingsValues(category, settingValues, policiesTypes, types);
            }

            return category;
        }

        public async Task<DynamicListViewModel> GetCategoryListByKey(string key)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, configurationContext);

            List<dynamic> values = null;

            switch (category.SourceKey)
            {
                case "Departments":
                    var departments = await _dataManager.GetAllDepartments();
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(departments));
                    break;

                case "ProcedureTypes":
                    var procedureTypes = await _dataManager.GetAllProcedureTypes();
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypes));
                    break;
                case "Labels":
                    var labels = await _dataManager.GetAllLabels();
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(labels));
                    break;
                default:
                    values = await _storageService.GetJsonDynamicList(category.SourceKey, 1, configurationContext);
                    break;

            }

            return values == null ? null : await BuildCategoryList(category, values);
        }

        public async Task<DynamicListViewModel> GetEmbeddedListByKey(string settingValues, string metadataKey, string listKey)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(metadataKey, 1, configurationContext);
            var values = SettingsHelper.GetEmbeddedList(listKey, settingValues);

            return await BuildCategoryList(category, values);
        }

        private async Task<DynamicListViewModel> BuildCategoryList(DynamicListViewModel category, List<dynamic> values) 
        {
            category.Data = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values));

            if (category.SaveAsFile)
            {
                foreach (var property in category.Properties)
                {
                    property.SourceValues = await GetPropertySources(property);
                }
            }
            else
            {
                foreach (var property in category.Properties)
                {
                    await SetIsRequired(configurationContext, category.SourceKey, property);

                    if (!string.IsNullOrEmpty(property.SourceKey))
                    {
                        property.SourceValues = await GetPropertySources(property);

                        if (property.SourceValues != null)
                        {
                            foreach (var element in category.Data)
                            {
                                SetRelatedObject(property, element);

                            }
                        }
                    }
                }
            }

            return category;
        }

        private static void SetRelatedObject(DynamicPropertyViewModel property, ExpandoObject element)
        {
            var serializedElement = JsonConvert.SerializeObject(element);
            JObject jsonElement = JObject.Parse(serializedElement);

            var id = jsonElement[property.JsonKey].ToString();

            string relatedObject =
                JsonConvert.SerializeObject(property.SourceValues.Where(s => s.Id == id));

            relatedObject = relatedObject == null || relatedObject == "null" ? "{}" : relatedObject;

            jsonElement.Add(new JProperty(property.JsonKeyForRelatedObject, JObject.Parse(relatedObject)));

            dynamic jsonElementObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonElement.ToString());

            var _original = (IDictionary<string, object>)jsonElementObject;
            var _clone = (IDictionary<string, object>)element;

            foreach (var propertyKey in _original)
                _clone[propertyKey.Key] = propertyKey.Value;
        }

        private async Task<IList<dynamic>> GetPropertySources(DynamicBaseSettingViewModel property)
        {
            if (string.IsNullOrEmpty(property.SourceKey))
            {
                switch (property.VisualStyle)
                {
                    case VisualStyles.FilePicker:
                        return GetFiles(property);

                    case VisualStyles.FolderPicker:
                        return GetFolders(property);
                }
            }
            else
            {
                switch (property.SourceKey)
                {
                    case "VideoSinks":
                        var videoSinks = await _storageService.GetJsonDynamicList(property.SourceKey, 1, configurationContext);

                        var dynamicVideoSinks = videoSinks
                            .Select(item =>
                            {
                                dynamic expandoObj = new ExpandoObject();
                                expandoObj.Id = item.Index;
                                expandoObj.Value = $"{item.Alias} ({item.Index})";
                                expandoObj.RelatedObject = item;
                                return (ExpandoObject)expandoObj;
                            })
                            .ToList();
                        return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicVideoSinks));

                    case "Departments":
                        var departments = await _dataManager.GetAllDepartments();
                        var dynamicDepartments = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(departments));
                        return GetDynamicList(property, dynamicDepartments);

                    default:
                        return await GetDynamicData(property);
                }
            }

            return null;
        }

        private async Task SetIsRequired(ConfigurationContext configurationContext, string key, DynamicPropertyViewModel item)
        {
            //It is a switch because this can grow on time
            switch (key)
            {
                case "DepartmentsMetadata":
                    switch (item.JsonKey)
                    {
                        case "DepartmentId":
                            var setupSettings = await _storageService.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, configurationContext);
                            bool departmentsSupported = setupSettings.General.DepartmentsSupported;                           
                            
                            item.Required = departmentsSupported;
                            item.IsHidden = !departmentsSupported;
                            item.IsAutoGenerated = !departmentsSupported;

                            break;
                    }
                    break;
            }
        }

        private async Task SetSettingsValues(DynamicSectionViewModel rootSection, string settingValues, 
            IList<KeyValuePairViewModel> policiesTypes, List<dynamic> types)
        {
            foreach (var section in rootSection.Sections)
            {
                await SetCategorySources(section, settingValues, types);

                SettingsHelper.SetSettingValues(section, settingValues, policiesTypes);

                if (section.Sections != null)
                {
                    await SetSettingsValues(section, settingValues, policiesTypes, types);
                }
            }
        }

        public async Task<List<dynamic>> GetListValues(string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            return await _storageService.GetJsonDynamicList(key, 1, configurationContext);
        }

        public async Task<dynamic> GetSettingValues(string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            return await _storageService.GetJsonDynamic(key, 1, configurationContext);
        }

        private async Task SetCategorySources(DynamicSectionViewModel category, string settingValues, List<dynamic> types)
        {
            if (category!= null && category.Settings != null)
            {
                foreach (var setting in category.Settings)
                {
                    if (string.IsNullOrEmpty(setting.SourceKey))
                    {
                        switch (setting.VisualStyle)
                        {
                            case VisualStyles.FilePicker:
                                setting.SourceValues = GetFiles(setting);
                                break;

                            case VisualStyles.FolderPicker:
                                setting.SourceValues = GetFolders(setting);
                                break;
                        }
                    }
                    else
                    { 
                        switch (setting.VisualStyle)
                        {
                            case VisualStyles.ExternalList:
                                setting.CustomList = await GetCategoryListByKey(setting.SourceKey);
                                break;

                            case VisualStyles.EmbeddedList:
                                setting.CustomList = await GetEmbeddedListByKey(settingValues, setting.SourceKey, setting.JsonKey);
                                break;

                            case VisualStyles.EmbeddedGenericList:
                                setting.SourceValues = AddTypes(SettingsHelper.GetEmbeddedList(setting.SourceKey, settingValues), types);
                                break;

                            case VisualStyles.DropDownEmbeddedList:
                            case VisualStyles.DropDownExternalList:
                                setting.SourceValues = await GetDynamicData(setting, settingValues);
                                break;

                            case VisualStyles.DropDownGenericList:
                            case VisualStyles.ExternalGenericList:
                                var values = await GetDynamicData(setting);
                                setting.SourceValues = AddTypes(values, types);
                                break;
                        }
                    }
                }
            }
        }

        private List<dynamic> AddTypes(List<dynamic> values, List<dynamic> types)
        {
            var dynamicValues = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values));
            dynamicValues.ToList().ForEach(s => s.TryAdd("Types", types));

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicValues));
        }

        private IList<dynamic> GetFiles(DynamicBaseSettingViewModel setting)
        {
            var files = _filesService.GetFiles(setting.Folder, setting.Filter);
            var sourceValuesFiles = files.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
            var dynamicFiles = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(sourceValuesFiles));

            var dynamicResult = dynamicFiles.Select(item =>
            {
                dynamic expandoObj = new ExpandoObject();
                expandoObj.Id = item.DisplayName;
                expandoObj.Value = item.DisplayName;
                expandoObj.RelatedObject = item;

                return (ExpandoObject)expandoObj;
            })
           .ToList();

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicResult));
        }

        private IList<dynamic> GetFolders(DynamicBaseSettingViewModel setting)
        {
            var folders = _filesService.GetFolders(setting.Folder);
            var sourceValuesFolders = folders.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
            var dynamicFolders = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(sourceValuesFolders));

            var dynamicResult = dynamicFolders.Select(item =>
            {
                dynamic expandoObj = new ExpandoObject();
                expandoObj.Id = item.DisplayName;
                expandoObj.Value = item.DisplayName;
                expandoObj.RelatedObject = item;

                return (ExpandoObject)expandoObj;
            })
            .ToList();

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicResult));
        }

        private async Task<List<dynamic>> GetDynamicData(DynamicBaseSettingViewModel setting, string settingValues = null)
        {
            if (setting.VisualStyle == VisualStyles.DropDownEmbeddedList 
                || setting.VisualStyle == VisualStyles.EmbeddedGenericList)
            {
                var values = SettingsHelper.GetEmbeddedList(setting.SourceKey, settingValues);
                return GetDynamicList(setting, values);
            }
            else
            {
                var list = await _storageService.GetJsonDynamicList(setting.SourceKey, 1, configurationContext);
                return GetDynamicList(setting, list);
            }
        }

        private static List<dynamic> GetDynamicList(DynamicBaseSettingViewModel setting, IList<dynamic> values)
        {
            var dynamicResult = values
                .Select(item =>
                {
                    dynamic expandoObj = new ExpandoObject();
                    expandoObj.Id = item[setting.SourceKeyId];
                    expandoObj.Value = item[setting.SourceKeyValue];
                    
                    if (!string.IsNullOrEmpty(setting.SourceKeyTranslationKey))
                        expandoObj.TranslationKey = item[setting.SourceKeyTranslationKey];

                    expandoObj.RelatedObject = item;
                    return (ExpandoObject)expandoObj;
                })
                .ToList();

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicResult));
        }

        private async Task SaveSources(DynamicSectionViewModel section, string settingsKey)
        {
            if (section.Settings != null)
            {
                foreach (var item in section.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        if (item.VisualStyle == VisualStyles.ExternalList)
                        {
                            var customList = item.CustomList;

                            if (customList.SaveAsFile)
                                await SaveCustomListFile(customList);
                            else
                                await SaveCustomEntities(user, customList);
                        }
                        else if (item.VisualStyle == VisualStyles.EmbeddedList)
                        {
                            var embeddedList = item.CustomList;
                            await SaveEmbeddedList(settingsKey, item.JsonKey, embeddedList);
                        }
                        else
                        {
                            await _storageService.SaveJsonObject(item.SourceKey, JsonConvert.SerializeObject(item.SourceValues), 1, configurationContext);
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
                await _storageService.SaveJsonObject(category.JsonKey, result, 1, configurationContext);
            }
            else
            {   
                throw new ValidationException("Json Schema Invalid for " + category.JsonKey);
            }
        }

        private async Task<bool> SchemaIsValid(string schemaKey, string json, ConfigurationContext configurationContext)
        {
            if (string.IsNullOrEmpty(schemaKey))
                return true;
            else
            {
                dynamic dynamicSchema = await _storageService.GetJsonFullDynamic(schemaKey, 1, configurationContext);

                if (dynamicSchema == null)
                    return true;
                else
                {
                    string schemaJson = JsonConvert.SerializeObject(dynamicSchema);
#pragma warning disable CS0618 // Type or member is obsolete
                    var schema = JsonSchema.Parse(schemaJson);
                    JObject jsonObject = JObject.Parse(json);
                    return jsonObject.IsValid(schema);
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }

        private async Task SaveCustomEntity(UserModel user, DynamicListViewModel customList, DynamicListActions action)
        {
            switch (customList.SourceKey)
            {
                case "Departments":
                    await SaveDepartments(action, customList.Entity);
                    break;
                case "ProcedureTypes":
                    await SaveProcedureTypes(action, customList.Entity);
                    break;
                case "Labels":
                    await SaveLabels(action, customList.Entity);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveCustomEntities(UserModel user, DynamicListViewModel category)
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
                case "Labels":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveLabels(DynamicListActions.Delete, item);
                    }
                    break;
            }
        }

        private async Task SaveProcedureTypes(DynamicListActions action, dynamic source)
        {
            var procedureType = new ProcedureTypeModel();

            if (SettingsHelper.IsPropertyExist(source.Department, "Id"))
                procedureType.DepartmentId = Convert.ToInt32(source.Department?.Id);

            SettingsHelper.Map(source, procedureType);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddProcedureType(procedureType);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteProcedureType(procedureType);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveDepartments(DynamicListActions action, dynamic source)
        {
            var department = new DepartmentModel();
            SettingsHelper.Map(source, department);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddDepartment(department);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteDepartment(department.Id);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveLabels(DynamicListActions action, dynamic source)
        {
            var label = new LabelModel();

            if (SettingsHelper.IsPropertyExist(source.Label, "Id"))
                label.ProcedureTypeId = source.ProcedureTypeId?.Id;

            SettingsHelper.Map(source, label);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddLabel(label);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteLabel(label);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }
    }
}
