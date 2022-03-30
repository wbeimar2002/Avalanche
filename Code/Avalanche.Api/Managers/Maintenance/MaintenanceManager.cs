using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Printing;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations.Media;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Avalanche.Api.Managers.Maintenance
{
    public abstract class MaintenanceManager : IMaintenanceManager
    {
        private readonly IStorageService _storageService;
        private readonly IDataManager _dataManager;
        private readonly IFilesService _filesService;
        private readonly IPrintingService _printingService;

        private readonly IMapper _mapper;
        private readonly ConfigurationContext _configurationContext;

        private readonly ISharedConfigurationManager _sharedConfigurationManager;

        protected MaintenanceManager(IStorageService storageService,
            IDataManager dataManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            IPrintingService printingService,
            ISharedConfigurationManager sharedConfigurationManager)
        {
            _storageService = storageService;
            _dataManager = dataManager;
            _mapper = mapper;
            _filesService = filesService;
            _printingService = printingService;

            _sharedConfigurationManager = sharedConfigurationManager;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        protected abstract void CheckLinks(DynamicListViewModel category);

        protected abstract Task SetIsRequired(string key, DynamicPropertyViewModel item);

        public async Task SaveCategoryPolicies(DynamicSectionViewModel category)
        {
            await SaveJsonValues(category, _configurationContext).ConfigureAwait(false);

            DynamicSettingsHelper.CleanSettings(category);

            await _storageService.SaveJsonMetadata(category.Metadata, JsonConvert.SerializeObject(category), 1, _configurationContext).ConfigureAwait(false);
        }

        public virtual async Task SaveCategory(DynamicSectionViewModel category)
        {
            await SaveSources(category, category.JsonKey).ConfigureAwait(false);

            if (category.Sections != null)
            {
                foreach (var section in category.Sections)
                {
                    await SaveSources(section, category.JsonKey).ConfigureAwait(false);
                }
            }

            await SaveJsonValues(category, _configurationContext).ConfigureAwait(false);
        }

        public async Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action)
        {
            if (category.SaveAsFile)
            {
                await SaveCustomListFile(category).ConfigureAwait(false);
            }
            else
            {
                await SaveCustomEntity(category, action).ConfigureAwait(false);
            }
        }

        public async Task<DynamicSectionViewModel> GetCategoryByKey(string key)
        {
            var category = await _storageService.GetJsonObject<DynamicSectionViewModel>(key, 1, _configurationContext).ConfigureAwait(false);
            var settingValues = await _storageService.GetJson(category.JsonKey, 1, _configurationContext).ConfigureAwait(false);

            var policiesTypes = await _storageService.GetJsonObject<List<KeyValuePairViewModel>>("SettingsPolicies", 1, _configurationContext).ConfigureAwait(false);
            var types = await GetList("SettingTypes").ConfigureAwait(false);

            await SetCategorySources(category, settingValues, types).ConfigureAwait(false);

            DynamicSettingsHelper.SetSettingValues(category, settingValues, policiesTypes);

            if (category.Sections != null)
            {
                await SetSettingsValues(category, settingValues, policiesTypes, types).ConfigureAwait(false);
            }

            return category;
        }

        public async Task<DynamicListViewModel> GetCategoryListByKey(string key, string parentId)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(key, 1, _configurationContext).ConfigureAwait(false);

            CheckLinks(category);

            List<dynamic> values = null;

            switch (category.SourceKey)
            {
                case "Users":
                    var users = await _dataManager.GetAllUsers().ConfigureAwait(false);
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(users));
                    break;

                case "Departments":
                    var departments = await _dataManager.GetAllDepartments().ConfigureAwait(false);
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(departments));
                    break;

                case "ProcedureTypesWithEmpty":
                    var procedureTypesWithEmpty = await _dataManager.GetAllProcedureTypes().ConfigureAwait(false);
                    //Added to follow business rule. This represents empty procedure type, used just in dropdown of labels
                    procedureTypesWithEmpty.Insert(0, new ProcedureTypeModel
                    {
                        Id = -1,
                        Name = "*"
                    });
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypesWithEmpty));
                    break;

                case "ProcedureTypes":
                    var procedureTypes = await _dataManager.GetAllProcedureTypes().ConfigureAwait(false);
                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypes));
                    break;

                case "Labels":
                    List<LabelModel> labels = null;

                    if (parentId == null)
                    {
                        labels = await _dataManager.GetAllLabels().ConfigureAwait(false);
                    }
                    else
                    {
                        labels = await _dataManager.GetLabelsByProcedureType(Convert.ToInt32(parentId)).ConfigureAwait(false);
                    }

                    values = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(labels));
                    break;

                default:
                    values = await _storageService.GetJsonDynamicList(category.SourceKey, 1, _configurationContext).ConfigureAwait(false);
                    break;
            }

            return values == null ? null : await BuildCategoryList(category, values).ConfigureAwait(false);
        }

        public async Task<DynamicListViewModel> GetEmbeddedListByKey(string settingValues, string metadataKey, string listKey)
        {
            var category = await _storageService.GetJsonObject<DynamicListViewModel>(metadataKey, 1, _configurationContext).ConfigureAwait(false);
            var values = DynamicSettingsHelper.GetEmbeddedList(listKey, settingValues);

            return await BuildCategoryList(category, values).ConfigureAwait(false);
        }

        private async Task<DynamicListViewModel> BuildCategoryList(DynamicListViewModel category, List<dynamic> values) 
        {
            category.Data = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values));

            if (category.SaveAsFile)
            {
                foreach (var property in category.Properties)
                {
                    property.SourceValues = await GetPropertySources(property).ConfigureAwait(false);
                }
            }
            else
            {
                foreach (var property in category.Properties)
                {
                    await SetIsRequired(category.SourceKey, property).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(property.SourceKey))
                    {
                        property.SourceValues = await GetPropertySources(property).ConfigureAwait(false);

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

        private void SetRelatedObject(DynamicPropertyViewModel property, ExpandoObject element)
        {
            var serializedElement = JsonConvert.SerializeObject(element);

            var jsonElement = JObject.Parse(serializedElement);

            var id = jsonElement[property.JsonKey];

            var found = property.SourceValues.FirstOrDefault(s => s.Id == id);

            if (found == null)
            {
                jsonElement.Add(new JProperty(property.JsonKeyForRelatedObject, JObject.Parse("{}")));
            }
            else
            {
                string relatedObject = JsonConvert.SerializeObject(found.RelatedObject);

                jsonElement.Add(new JProperty(property.JsonKeyForRelatedObject, JObject.Parse(relatedObject)));

                dynamic jsonElementObject = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(jsonElement));

                var original = (IDictionary<string, object>)jsonElementObject;
                var clone = (IDictionary<string, object>)element;

                foreach (var propertyKey in original)
                {
                    clone[propertyKey.Key] = propertyKey.Value;
                }
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
                    case "GpioPins":
                        var gpioPins = await _dataManager.GetGpioPins().ConfigureAwait(false);

                        var dynamicGpioPins = gpioPins
                            .Select(item =>
                            {
                                dynamic expandoObj = new ExpandoObject();
                                expandoObj.Id = item.Index;
                                expandoObj.Value = $"{item.Alias} ({item.Index})";
                                expandoObj.RelatedObject = item;
                                return (ExpandoObject)expandoObj;
                            })
                            .ToList();

                        return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicGpioPins));

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

                    case "MediaActions":
                        var dynamicMediaActions = Enum.GetValues(typeof(GpioAction))
                            .Cast<GpioAction>()
                            .Select(item =>
                            {
                                dynamic expandoObj = new ExpandoObject();
                                expandoObj.Id = item.ToString();
                                expandoObj.Value = item.ToString();
                                expandoObj.TranslationKey = "mediaActions." + item.ToString();
                                expandoObj.RelatedObject = item;
                                return (ExpandoObject)expandoObj;
                            })
                            .ToList();

                        return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicMediaActions));



                    case "Departments":
                        var departments = await _dataManager.GetAllDepartments().ConfigureAwait(false);
                        var dynamicDepartments = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(departments));
                        return GetDynamicList(property, dynamicDepartments);

                    case "ProcedureTypesWithEmpty":
                    case "ProcedureTypes":
                        var procedureTypes = await _dataManager.GetAllProcedureTypes().ConfigureAwait(false);
                        //Added to follow business rule. This represents empty procedure type, used just in dropdown of labels
                        procedureTypes.Insert(0, new ProcedureTypeModel
                        {
                            Id = -1,
                            Name = "*"
                        });
                        var dynamicProcedureTypes = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(procedureTypes));
                        return GetDynamicList(property, dynamicProcedureTypes);

                    default:
                        return await GetDynamicData(property).ConfigureAwait(false);
                }
            }

            return null;
        }

        private async Task SetSettingsValues(DynamicSectionViewModel rootSection, string settingValues,
            IList<KeyValuePairViewModel> policiesTypes, List<dynamic> types)
        {
            foreach (var section in rootSection.Sections)
            {
                await SetCategorySources(section, settingValues, types).ConfigureAwait(false);

                DynamicSettingsHelper.SetSettingValues(section, settingValues, policiesTypes);

                if (section.Sections != null)
                {
                    await SetSettingsValues(section, settingValues, policiesTypes, types).ConfigureAwait(false);
                }
            }
        }

        public async Task<dynamic> GetSettingValues(string key) =>
            await _storageService.GetJsonDynamic(key, 1, _configurationContext).ConfigureAwait(false);

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
                                setting.CustomList = await GetCategoryListByKey(setting.SourceKey, null).ConfigureAwait(false);
                                break;

                            case VisualStyles.EmbeddedList:
                                setting.CustomList = await GetEmbeddedListByKey(settingValues, setting.SourceKey, setting.JsonKey).ConfigureAwait(false);
                                break;

                            case VisualStyles.EmbeddedGenericList:
                                setting.SourceValues = AddTypes(DynamicSettingsHelper.GetEmbeddedList(setting.SourceKey, settingValues), types);
                                break;

                            case VisualStyles.DropDownExternalEmbeddedList:
                                var externalKey = setting.Source;
                                var externalSettingValues = await _storageService.GetJson(externalKey, 1, _configurationContext).ConfigureAwait(false);
                                var externalValues = DynamicSettingsHelper.GetEmbeddedList(setting.SourceKey, externalSettingValues);
                                setting.SourceValues = GetDynamicList(setting, externalValues);
                                break;

                            case VisualStyles.DropDownEmbeddedList:
                            case VisualStyles.DropDownExternalList:
                                setting.SourceValues = await GetDynamicData(setting, settingValues).ConfigureAwait(false);
                                break;

                            case VisualStyles.DropDownGenericList:
                            case VisualStyles.ExternalGenericList:
                                var values = await GetDynamicData(setting).ConfigureAwait(false);
                                setting.SourceValues = AddTypes(values, types);
                                break;
                            case VisualStyles.DropDownCustomList:
                                var customValues = await GetCustomValues(setting).ConfigureAwait(false);
                                setting.SourceValues = GetDynamicList(setting, customValues);
                                break;
                        }
                    }
                }
            }
        }

        private async Task<List<dynamic>> GetCustomValues(DynamicSettingViewModel setting)
        {
            switch (setting.SourceKey)
            {
                case "Printers":
                    var printersResponse = await _printingService.GetPrinters().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(printersResponse.Printers.Select(p => new { Name = p })));
                case "Medpresence-Environments":
                    return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(new List<object> { new { Name = "Debug" }, new { Name = "Te" }, new { Name = "Prod" } }));
                default:
                    break;
            }

            return new List<dynamic>();
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

            var dynamicResult = dynamicFiles.ConvertAll(item =>
            {
                dynamic expandoObj = new ExpandoObject();
                expandoObj.Id = item.DisplayName;
                expandoObj.Value = item.DisplayName;
                expandoObj.RelatedObject = item;

                return (ExpandoObject)expandoObj;
            });

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicResult));
        }

        private IList<dynamic> GetFolders(DynamicBaseSettingViewModel setting)
        {
            var folders = _filesService.GetFolders(setting.Folder);
            var sourceValuesFolders = folders.Select(x => new FileSystemElementViewModel() { DisplayName = x, IsFolder = false });
            var dynamicFolders = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(sourceValuesFolders));

            var dynamicResult = dynamicFolders.ConvertAll(item =>
            {
                dynamic expandoObj = new ExpandoObject();
                expandoObj.Id = item.DisplayName;
                expandoObj.Value = item.DisplayName;
                expandoObj.RelatedObject = item;

                return (ExpandoObject)expandoObj;
            });

            return JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(dynamicResult));
        }

        private async Task<List<dynamic>> GetDynamicData(DynamicBaseSettingViewModel setting, string settingValues = null)
        {
            if (setting.VisualStyle == VisualStyles.DropDownEmbeddedList
                || setting.VisualStyle == VisualStyles.EmbeddedGenericList)
            {
                var values = DynamicSettingsHelper.GetEmbeddedList(setting.SourceKey, settingValues);
                return GetDynamicList(setting, values);
            }

            var list = await _storageService.GetJsonDynamicList(setting.SourceKey, 1, _configurationContext).ConfigureAwait(false);

            return GetDynamicList(setting, list);
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
                    {
                        expandoObj.TranslationKey = item[setting.SourceKeyTranslationKey];
                    }

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
                            {
                                await SaveCustomListFile(customList).ConfigureAwait(false);
                            }
                            else
                            {
                                await SaveCustomEntities(customList).ConfigureAwait(false);
                            }
                        }
                        else if (item.VisualStyle == VisualStyles.EmbeddedList)
                        {
                            await SaveEmbeddedList(settingsKey, item.JsonKey, JsonConvert.SerializeObject(item.CustomList.Data), item.CustomList.Schema).ConfigureAwait(false);
                        }
                        else if (item.VisualStyle == VisualStyles.EmbeddedGenericList)
                        {
                            await SaveEmbeddedList(settingsKey, item.SourceKey, JsonConvert.SerializeObject(item.SourceValues)).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private async Task SaveJsonValues(DynamicSectionViewModel category, ConfigurationContext configurationContext)
        {
            var json = DynamicSettingsHelper.GetJsonValues(category);

            if (await _storageService.ValidateSchema(category.Schema, json, 1, configurationContext).ConfigureAwait(false))
            {
                await _storageService.SaveJsonObject(category.JsonKey, json, 1, configurationContext).ConfigureAwait(false);

                switch (category.JsonKey)
                {
                    case "PrintingConfiguration":
                        _sharedConfigurationManager.UseVSSPrintingService(json.Get<PrintingConfiguration>().UseVSSPrintingService);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + category.JsonKey);
            }
        }

        private async Task SaveCustomEntity(DynamicListViewModel customList, DynamicListActions action)
        {
            switch (customList.SourceKey)
            {
                case "Departments":
                    await SaveDepartment(action, customList.Entity).ConfigureAwait(false);
                    break;
                case "ProcedureTypes":
                    await SaveProcedureType(action, customList.Entity).ConfigureAwait(false);
                    break;
                case "Labels":
                    await SaveLabel(action, customList.Entity).ConfigureAwait(false);
                    break;
                case "Users":
                    await SaveUser(action, customList.Entity).ConfigureAwait(false);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveCustomEntities(DynamicListViewModel category)
        {
            switch (category.SourceKey)
            {
                case "Departments":
                    foreach (var item in category.NewData)
                    {
                        await SaveDepartment(DynamicListActions.Insert, item).ConfigureAwait(false);
                    }
                    break;
                case "ProcedureTypes":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveDepartment(DynamicListActions.Delete, item).ConfigureAwait(false);
                    }
                    break;
                case "Labels":
                    foreach (var item in category.DeletedData)
                    {
                        await SaveLabel(DynamicListActions.Delete, item).ConfigureAwait(false);
                    }
                    break;
            }
        }

        private async Task SaveUser(DynamicListActions action, dynamic source)
        {
            var user = new UserModel();

            DynamicSettingsHelper.Map(source, user);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddUser(user).ConfigureAwait(false);
                    break;
                case DynamicListActions.Update:
                    await _dataManager.UpdateUser(user).ConfigureAwait(false);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteUser(user.Id.Value).ConfigureAwait(false);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveLabel(DynamicListActions action, dynamic source)
        {
            var label = new LabelModel();

            DynamicSettingsHelper.Map(source, label);

            if (DynamicSettingsHelper.PropertyExists(source, "ProcedureType") && DynamicSettingsHelper.PropertyExists(source.ProcedureType, "Id"))
            {
                label.ProcedureTypeId = Convert.ToInt32(source.ProcedureType?.Id);

                if (label.ProcedureTypeId <= 0)
                    label.ProcedureTypeId = null;
            }

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddLabel(label).ConfigureAwait(false);
                    break;
                case DynamicListActions.Update:
                    await _dataManager.UpdateLabel(label).ConfigureAwait(false);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteLabel(label).ConfigureAwait(false);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveProcedureType(DynamicListActions action, dynamic source)
        {
            var procedureType = new ProcedureTypeModel();

            DynamicSettingsHelper.Map(source, procedureType);

            if (DynamicSettingsHelper.PropertyExists(source, "Department") && DynamicSettingsHelper.PropertyExists(source.Department, "Id"))
                procedureType.DepartmentId = Convert.ToInt32(source.Department?.Id);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddProcedureType(procedureType).ConfigureAwait(false);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteProcedureType(procedureType).ConfigureAwait(false);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveDepartment(DynamicListActions action, dynamic source)
        {
            var department = new DepartmentModel();

            DynamicSettingsHelper.Map(source, department);

            switch (action)
            {
                case DynamicListActions.Insert:
                    await _dataManager.AddDepartment(department).ConfigureAwait(false);
                    break;
                case DynamicListActions.Delete:
                    await _dataManager.DeleteDepartment(department.Id).ConfigureAwait(false);
                    break;
                default:
                    throw new ValidationException("Method Not Allowed");
            }
        }

        private async Task SaveCustomListFile(DynamicListViewModel customList)
        {
            var json = JsonConvert.SerializeObject(customList.Data);

            if (await _storageService.ValidateSchema(customList.Schema, json, 1, _configurationContext).ConfigureAwait(false))
            {
                await _storageService.SaveJsonObject(customList.SourceKey, json, 1, _configurationContext, true).ConfigureAwait(false);
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + customList.SourceKey);
            }
        }

        private async Task<List<dynamic>> GetList(string sourceKey, string jsonKey = null)
        {
            if (jsonKey == null)
            {
                return await _storageService.GetJsonDynamicList(sourceKey, 1, _configurationContext).ConfigureAwait(false);
            }

            var settingValues = await _storageService.GetJson(sourceKey, 1, _configurationContext).ConfigureAwait(false);
            return DynamicSettingsHelper.GetEmbeddedList(jsonKey, settingValues);
        }

        private async Task SaveEmbeddedList(string settingsKey, string jsonKey, string json, string schema = null)
        {
            if (string.IsNullOrEmpty(schema) || await _storageService.ValidateSchema(schema, json, 1, _configurationContext).ConfigureAwait(false))
            {
                await _storageService.UpdateJsonProperty(settingsKey, jsonKey, json, 1, _configurationContext, true).ConfigureAwait(false);

                switch (settingsKey)
                {
                    case "ProceduresSearchConfiguration":
                        _sharedConfigurationManager.UpdateProceduresSearchConfigurationColumns(json.Get<List<ColumnProceduresSearchConfiguration>>());
                        break;
                    case "SetupConfiguration":
                        _sharedConfigurationManager.UpdatePatientInfo(json.Get<List<PatientInfoSetupConfiguration>>());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + jsonKey);
            }
        }
    }
}
