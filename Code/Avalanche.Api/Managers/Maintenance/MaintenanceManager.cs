using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
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
        private readonly IDataManager _metadataManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILibraryService _libraryService;
        private readonly IFilesService _filesService;

        private readonly UserModel user;
        private readonly ConfigurationContext configurationContext;

        public MaintenanceManager(IStorageService storageService, 
            IDataManager metadataManager, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService)
        {
            _storageService = storageService;
            _metadataManager = metadataManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _libraryService = libraryService;
            _filesService = filesService;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
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
                //TODO: Pending Exceptions strategy
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
                //TODO: Pending Exceptions strategy
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        public async Task<DynamicSectionViewModel> GetCategoryByKey(string key)
        {
            var configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            var category = await _storageService.GetJsonObject<DynamicSectionViewModel>(key, 1, configurationContext);
            var settingValues = await _storageService.GetJson(category.JsonKey, 1, configurationContext);

            var types = await _metadataManager.GetData(DataTypes.SettingTypes);
            var policiesTypes = (await _storageService.GetJsonObject<List<KeyValuePairViewModel>>("SettingsPolicies", 1, configurationContext));           

            await SetSources(category, types, settingValues);

            SettingsHelper.SetSettingValues(category, settingValues, policiesTypes);

            if (category.Sections != null)
            {
                await SetSettingsValues(category, settingValues, types, policiesTypes);
            }

            return category;
        }

        public async Task<DynamicListViewModel> GetCategoryListByKey(string key)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, configurationContext);
            var values = await _storageService.GetJsonObject<List<ExpandoObject>>(category.SourceKey, 1, configurationContext);

            return await BuildCategoryList(category, values);
        }

        public async Task<DynamicListViewModel> GetEmbeddedListByKey(string settingValues, string metadataKey, string listKey)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(metadataKey, 1, configurationContext);
            var values = SettingsHelper.GetEmbeddedList(listKey, settingValues);

            return await BuildCategoryList(category, JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values)));
        }

        private async Task<DynamicListViewModel> BuildCategoryList(DynamicListViewModel category, List<ExpandoObject> values) 
        { 
            if (category.SaveAsFile)
            {
                category.Data = values;

                foreach (var item in category.Properties)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = await GetDynamicSourceValues(item);
                    }
                }
            }
            else
            {
                category.Data = await GetDynamicData(category.SourceKey);

                foreach (var item in category.Properties)
                {
                    await SetIsRequired(configurationContext, category.SourceKey, item);

                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        item.SourceValues = await GetDynamicSourceValues(item);

                        foreach (var element in category.Data)
                        {
                            var serializedElement = JsonConvert.SerializeObject(element);
                            JObject jsonElement = JObject.Parse(serializedElement);

                            string relatedObject =
                                JsonConvert.SerializeObject(item.SourceValues.Where(s => s.Id == jsonElement[item.JsonKey].ToString()).FirstOrDefault()?.RelatedObject);

                            relatedObject = relatedObject == null || relatedObject == "null" ? "{}" : relatedObject;

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

        public async Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest)
        {
            Preconditions.ThrowIfNull(nameof(reindexRequest), reindexRequest);
            Preconditions.ThrowIfNullOrEmpty(nameof(reindexRequest.RepositoryName), reindexRequest.RepositoryName);

            var response = await _libraryService.ReindexRepository(reindexRequest.RepositoryName);
            return _mapper.Map<ReindexStatusViewModel>(response);
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
                            dynamic setupSettings = await _storageService.GetJsonDynamic("SetupSettingsValues", 1, configurationContext);
                            bool departmentsSupported = setupSettings.General.DepartmentsSupported;                           
                            
                            item.Required = departmentsSupported;
                            item.IsHidden = !departmentsSupported;
                            item.IsAutoGenerated = !departmentsSupported;

                            break;
                    }
                    break;
            }
        }

        private async Task<IList<KeyValuePairObjectViewModel>> GetDynamicSourceValues(DynamicPropertyViewModel dynamicPropertyViewModel)
        {
            switch (dynamicPropertyViewModel.SourceKey)
            {
                case "Departments":

                    var departments = await GetDynamicData("Departments");
                    return departments.Select(d => new KeyValuePairObjectViewModel()
                    {
                        Id = ((dynamic)d).Id.ToString(),
                        Value = ((dynamic)d).Name,
                        RelatedObject = d
                    }).ToList();

                case "PgsVideoFiles":

                    var videoFiles = _filesService.GetFiles("pgsmedia", "*.mp4");

                    var dynamicVideoFiles = videoFiles
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.FileName = item;
                            expandoObj.Icon = dynamicPropertyViewModel.Icon;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();

                    return dynamicVideoFiles.Select(d => new KeyValuePairObjectViewModel()
                    {
                        Id = ((dynamic)d).FileName,
                        Value = ((dynamic)d).FileName,
                        RelatedObject = d
                    }).ToList();

                case "Sinks":

                    var sinks = (await _storageService.GetJsonObject<List<ExpandoObject>>("Sinks", 1, configurationContext));
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

        private async Task SetSettingsValues(DynamicSectionViewModel rootSection, string settingValues, IList<KeyValuePairViewModel> types, IList<KeyValuePairViewModel> policiesTypes)
        {
            foreach (var section in rootSection.Sections)
            {
                await SetSources(section, types, settingValues);

                SettingsHelper.SetSettingValues(section, settingValues, policiesTypes);

                if (section.Sections != null)
                {
                    await SetSettingsValues(section, settingValues, types, policiesTypes);
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

        private async Task SetSources(DynamicSectionViewModel category, IList<KeyValuePairViewModel> types, string settingValues)
        {
            if (category!= null && category.Settings != null)
            {
                foreach (var item in category.Settings)
                {
                    if (!string.IsNullOrEmpty(item.SourceKey))
                    {
                        switch (item.VisualStyle)
                        {
                            case VisualStyles.ExternalList:
                                item.CustomList = await GetCategoryListByKey(item.SourceKey);
                                break;

                            case VisualStyles.EmbeddedList:
                                item.CustomList = await GetEmbeddedListByKey(settingValues, item.SourceKey, item.JsonKey);
                                break;

                            case VisualStyles.FilePicker:

                                var files = _filesService.GetFiles(item.Folder, item.Filter);
                                var sourceValuesFiles = files.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
                                item.SourceValues = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(sourceValuesFiles));
                                break;

                            case VisualStyles.FolderPicker:

                                var folders = _filesService.GetFolders(item.Folder);
                                var sourceValuesFolders = folders.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
                                item.SourceValues = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(sourceValuesFolders));
                                break;

                            case VisualStyles.DropDownEmbeddedList:
                                item.SourceValues = await GetDynamicData(item.SourceKey, settingValues);
                                break;

                            case VisualStyles.DropDownExternalList:
                                item.SourceValues = await GetDynamicData(item.SourceKey);
                                break;

                            case VisualStyles.DropDownGenericList:
                                item.SourceValues = await GetDynamicData(item.SourceKey);
                                item.SourceValues.ToList().ForEach(s => s.TryAdd("Types", types));
                                break;
                        }
                    }
                }
            }
        }

        private async Task<List<ExpandoObject>> GetDynamicData(string sourceKey, string settingValues = null)
        {
            switch (sourceKey)
            {
                case "Departments":
                    var departments = await _metadataManager.GetAllDepartments();

                    return departments
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.Id = item.Id;
                            expandoObj.Name = item.Name;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();

                case "ProcedureTypes":
                    var procedureTypes = await _metadataManager.GetAllProcedureTypes();

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

                case "RoutingModes":

                    var routingModes = SettingsHelper.GetEmbeddedList(sourceKey, settingValues);

                    return routingModes
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.Id = item.Id;
                            expandoObj.Value = item.Name;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();
                    
                default:
                    var list = await _storageService.GetJsonDynamicList(sourceKey, 1, configurationContext);

                    return list
                        .Select(item =>
                        {
                            dynamic expandoObj = new ExpandoObject();
                            expandoObj.Id = item.Id;
                            expandoObj.Value = item.Name;
                            return (ExpandoObject)expandoObj;
                        })
                        .ToList();
            }
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
                //TODO: Pending Exceptions strategy
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
                    var schema = JsonSchema.Parse(schemaJson);

                    JObject jsonObject = JObject.Parse(json);

                    return jsonObject.IsValid(schema);
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
                default:
                    //TODO: Pending Exceptions strategy
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
            var department = new DepartmentModel();
            SettingsHelper.Map(source, department);

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
    }
}
