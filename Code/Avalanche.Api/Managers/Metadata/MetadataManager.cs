using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Core.DataManagement.V1.Protos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        readonly IStorageService _storageService;
        readonly IMapper _mapper;
        readonly IDataManagementService _dataManagementService;

        public MetadataManager(IStorageService storageService,
            IDataManagementService dataManagementService,
            IMapper mapper)
        {
            _dataManagementService = dataManagementService;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<IList<KeyValuePairViewModel>> GetMetadata(User user, MetadataTypes type)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);

            switch (type)
            {
                case MetadataTypes.Sex:
                    return (await _storageService.GetJsonObject<ListContainerViewModel>("SexTypes", 1, configurationContext)).Items;
                case MetadataTypes.SourceTypes:
                    return (await _storageService.GetJsonObject<ListContainerViewModel>("SourceTypes", 1, configurationContext)).Items;
                case MetadataTypes.SettingTypes:
                    return (await _storageService.GetJsonObject<ListContainerViewModel>("SettingTypes", 1, configurationContext)).Items;
                case MetadataTypes.PgsVideoFiles:
                    return (await _storageService.GetJsonObject<ListContainerViewModel>("PgsVideoFiles", 1, configurationContext)).Items;
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        public async Task<IList<DynamicSourceKeyValuePairViewModel>> GetSource(User user, MetadataTypes type)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);

            switch (type)
            {
                case MetadataTypes.SearchColumns:
                    return (await _storageService.GetJsonObject<SourceListContainerViewModel>("SearchColumns", 1, configurationContext)).Items;
                case MetadataTypes.PgsVideoFiles:
                    return (await _storageService.GetJsonObject<SourceListContainerViewModel>("PgsVideoFiles", 1, configurationContext)).Items;
                default:
                    return new List<DynamicSourceKeyValuePairViewModel>();
            }
        }

        public async Task<ExpandoObject> GetDynamicSource(User user, string key)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var dynamicObject = await _storageService.GetJsonDynamic(key, 1, configurationContext);
            return JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(dynamicObject));
        }

        public async Task<Department> AddDepartment(Avalanche.Shared.Domain.Models.User user, Department department)
        {
            await ValidateDepartmentsSupport(user);
            Preconditions.ThrowIfNull(nameof(department.Name), department.Name);

            var result = await _dataManagementService.AddDepartment(_mapper.Map<Department, AddDepartmentRequest>(department));
            return _mapper.Map<AddDepartmentResponse, Department>(result);
        }

        public async Task DeleteDepartment(Avalanche.Shared.Domain.Models.User user, int departmentId)
        {
            await ValidateDepartmentsSupport(user);

            await _dataManagementService.DeleteDepartment(new DeleteDepartmentRequest() { DepartmentId = departmentId });
        }

        public async Task<IList<Department>> GetAllDepartments(User user)
        {
            await ValidateDepartmentsSupport(user);

            var result = await _dataManagementService.GetAllDepartments();

            return _mapper.Map<IList<DepartmentMessage>, IList<Department>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureType> AddProcedureType(User user, ProcedureType procedureType)
        {
            await ValidateDepartmentsSupport(user, procedureType.DepartmentId);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            var result = await _dataManagementService.AddProcedureType(_mapper.Map<ProcedureType, AddProcedureTypeRequest>(procedureType));
            return _mapper.Map<AddProcedureTypeResponse, ProcedureType>(result);
        }

        public async Task DeleteProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType)
        {
            await ValidateDepartmentsSupport(user, procedureType.DepartmentId);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            await _dataManagementService.DeleteProcedureType(_mapper.Map<ProcedureType, DeleteProcedureTypeRequest>(procedureType));
        }

        public async Task<List<ProcedureType>> GetProcedureTypesByDepartment(Avalanche.Shared.Domain.Models.User user, int? departmentId)
        {
            await ValidateDepartmentsSupport(user, departmentId);

            var result = await _dataManagementService.GetProceduresByDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.GetProceduresByDepartmentRequest()
            {
                DepartmentId = departmentId
            });

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureType>>(result.ProcedureTypeList).ToList();
        }

        public async Task ValidateDepartmentsSupport(User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            dynamic setupSettings = await _storageService.GetJsonDynamic("SetupSettingsValues", 1, configurationContext);

            bool departmentSupported = setupSettings.General.DepartmentsSupported;
            if (!departmentSupported)
            {
                throw new System.InvalidOperationException("Departments are not supported");
            }
        }

        public async Task ValidateDepartmentsSupport(User user, int? departmentId)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var setupSettings = await _storageService.GetJsonDynamic("SetupSettingsValues", 1, configurationContext);

            bool departmentSupported = setupSettings.General.DepartmentsSupported;
#warning TODO: Check the strategy to throw business logic exceptions. Same exceptions in Patients Manager
            if (departmentSupported)
            {
                if (departmentId == null || departmentId == 0)
                    throw new System.ArgumentNullException("Department value is invalid. It should not be null. Departments are supported.");
            }
            else
            {
                if (departmentId != null && departmentId != 0)
                    throw new System.ArgumentException("Department value is invalid. Departments are not supported.");
            }
        }
    }
}