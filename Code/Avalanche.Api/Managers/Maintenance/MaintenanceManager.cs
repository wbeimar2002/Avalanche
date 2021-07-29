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

        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly GeneralApiConfiguration _generalApiConfiguration;

        public MaintenanceManager(IStorageService storageService, 
            IDataManager dataManager, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            GeneralApiConfiguration generalApiConfiguration)
        {
            _storageService = storageService;
            _dataManager = dataManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _libraryService = libraryService;
            _filesService = filesService;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
            _generalApiConfiguration = generalApiConfiguration;
        }

        public async Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest)
        {
            Preconditions.ThrowIfNull(nameof(reindexRequest), reindexRequest);
            Preconditions.ThrowIfNullOrEmpty(nameof(reindexRequest.RepositoryName), reindexRequest.RepositoryName);

            var response = await _libraryService.ReindexRepository(reindexRequest.RepositoryName);
            return _mapper.Map<ReindexStatusViewModel>(response);
        }

        public async Task SaveCategoryPolicies(DynamicSectionViewModel category)
        {
            await SaveJsonValues(category, _configurationContext);

            SettingsHelper.CleanSettings(category);

            await _storageService.SaveJsonMetadata(category.Metadata, JsonConvert.SerializeObject(category), 1, _configurationContext);
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

            await SaveJsonValues(category, _configurationContext);
        }

        public async Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action)
        {
            if (category.SaveAsFile)
                await SaveCustomListFile(category);
            else
                await SaveCustomEntity(_user, category, action);
        }

        private async Task SaveEmbeddedList(string settingsKey, string jsonKey, DynamicListViewModel customList)
        {
            var json = JsonConvert.SerializeObject(customList.Data);

            if (await _storageService.ValidateSchema(customList.Schema, json, 1, _configurationContext))
            {
                await _storageService.UpdateJsonProperty(settingsKey, jsonKey, json, 1, _configurationContext, true);
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        private async Task SaveCustomListFile(DynamicListViewModel customList)
        {
            var json = JsonConvert.SerializeObject(customList.Data);

            if (await _storageService.ValidateSchema(customList.Schema, json, 1, _configurationContext))
            {
                await _storageService.SaveJsonObject(customList.SourceKey, json, 1, _configurationContext, true);
            }
            else
            {   
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        public async Task<DynamicSectionViewModel> GetCategoryByKey(string key)
        {
            var configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
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

        public async Task<DynamicListViewModel> GetCategoryListByKey(string key, string parentId)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, _configurationContext);

            List<dynamic> values = null;

            switch (category.SourceKey)
            {
                case "Departments":
                    var departments = await _dataManager.GetAllDepartments();
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(departments));
                    break;

                case "ProcedureTypesWithEmpty":
                    var procedureTypesWithEmpty = await _dataManager.GetAllProcedureTypes();
                    //Added to follow business rule. This represents empty procedure type, used just in dropdown of labels
                    procedureTypesWithEmpty.Insert(0, new ProcedureTypeModel
                    {
                        Id = -1,
                        Name = "*"
                    });
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypesWithEmpty));
                    break;

                case "ProcedureTypes":
                    var procedureTypes = await _dataManager.GetAllProcedureTypes();
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypes));
                    break;

                case "Labels":
                    List<LabelModel> labels = null;

                    if (parentId == null)
                        labels = await _dataManager.GetAllLabels();
                    else
                        labels = await _dataManager.GetLabelsByProcedureType(Convert.ToInt32(parentId));

                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(labels));
                    break;

                default:
                    values = await _storageService.GetJsonDynamicList(category.SourceKey, 1, _configurationContext);
                    break;

            }

            return values == null ? null : await BuildCategoryList(category, values);
        }

        public async Task<DynamicListViewModel> GetEmbeddedListByKey(string settingValues, string metadataKey, string listKey)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(metadataKey, 1, _configurationContext);
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
                    await SetIsRequired(_configurationContext, category.SourceKey, property);

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

            var id = jsonElement[property.JsonKey];

            var found = property.SourceValues.Where(s => s.Id == id).FirstOrDefault();

            if (found == null)
            {
                jsonElement.Add(new JProperty(property.JsonKeyForRelatedObject, JObject.Parse("{}")));                
            }
            else
            {
                string relatedObject = JsonConvert.SerializeObject(found.RelatedObject);

                jsonElement.Add(new JProperty(property.JsonKeyForRelatedObject, JObject.Parse(relatedObject)));

                dynamic jsonElementObject = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(jsonElement));

                var _original = (IDictionary<string, object>)jsonElementObject;
                var _clone = (IDictionary<string, object>)element;

                foreach (var propertyKey in _original)
                    _clone[propertyKey.Key] = propertyKey.Value;
            }
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
                        var videoSinks = await _storageService.GetJsonDynamicList(property.SourceKey, 1, _configurationContext);

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

                    case "ProcedureTypesWithEmpty":
                    case "ProcedureTypes":
                        var procedureTypes = await _dataManager.GetAllProcedureTypes();
                        //Added to follow business rule. This represents empty procedure type, used just in dropdown of labels
                        procedureTypes.Insert(0, new ProcedureTypeModel
                        {
                            Id = -1,
                            Name = "*"
                        });
                        var dynamicProcedureTypes = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypes));
                        return GetDynamicList(property, dynamicProcedureTypes);

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
            var configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(_user);
            return await _storageService.GetJsonDynamicList(key, 1, configurationContext);
        }

        public async Task<dynamic> GetSettingValues(string key)
        {
            var configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(_user);
            return await _storageService.GetJsonDynamic(key, 1, configurationContext);
        }

        public async Task<GeneralApiConfiguration> GetGeneralApiConfigurationSettings()
        {
            return _generalApiConfiguration;
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
                                setting.CustomList = await GetCategoryListByKey(setting.SourceKey, null);
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
                var list = await _storageService.GetJsonDynamicList(setting.SourceKey, 1, _configurationContext);
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
                                await SaveCustomEntities(_user, customList);
                        }
                        else if (item.VisualStyle == VisualStyles.EmbeddedList)
                        {
                            var embeddedList = item.CustomList;
                            await SaveEmbeddedList(settingsKey, item.JsonKey, embeddedList);
                        }
                        else
                        {
                            await _storageService.SaveJsonObject(item.SourceKey, JsonConvert.SerializeObject(item.SourceValues), 1, _configurationContext);
                            item.SourceValues = null;
                        }
                    }
                }
            }
        }

        private async Task SaveJsonValues(DynamicSectionViewModel category, ConfigurationContext configurationContext)
        {
            string json = SettingsHelper.GetJsonValues(category);

            if (await _storageService.ValidateSchema(category.Schema, json, 1, configurationContext))
            {
                await _storageService.SaveJsonObject(category.JsonKey, json, 1, configurationContext);
            }
            else
            {   
                throw new ValidationException("Json Schema Invalid for " + category.JsonKey);
            }
        }

        private async Task SaveCustomEntity(UserModel user, DynamicListViewModel customList, DynamicListActions action)
        {
            switch (customList.SourceKey)
            {
                case "Departments":
                    await SaveDepartment(action, customList.Entity);
                    break;
                case "ProcedureTypes":
                    await SaveProcedureType(action, customList.Entity);
                    break;
                case "Labels":
                    await SaveLabel(action, customList.Entity);
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
                        await SaveDepartment(DynamicListActions.Insert, item);
                    }
                    break;
                case "ProcedureTypes":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveDepartment(DynamicListActions.Delete, item);
                    }
                    break;
                case "Labels":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveLabel(DynamicListActions.Delete, item);
                    }
                    break;
            }
        }

        private async Task SaveLabel(DynamicListActions action, dynamic source)
        {
            var label = new LabelModel();

            SettingsHelper.Map(source, label);

            if (SettingsHelper.PropertyExists(source, "ProcedureType") && SettingsHelper.PropertyExists(source.ProcedureType, "Id"))
            {
                label.ProcedureTypeId = Convert.ToInt32(source.ProcedureType?.Id);

                if (label.ProcedureTypeId <= 0)
                    label.ProcedureTypeId = null;
            }

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddLabel(label);
                    break;
                case DynamicListActions.Update:
                    await _dataManager.UpdateLabel(label);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteLabel(label);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveProcedureType(DynamicListActions action, dynamic source)
        {
            var procedureType = new ProcedureTypeModel();

            SettingsHelper.Map(source, procedureType);

            if (SettingsHelper.PropertyExists(source, "Department") && SettingsHelper.PropertyExists(source.Department, "Id"))
                procedureType.DepartmentId = Convert.ToInt32(source.Department?.Id);

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

        private async Task SaveDepartment(DynamicListActions action, dynamic source)
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
    }
}
