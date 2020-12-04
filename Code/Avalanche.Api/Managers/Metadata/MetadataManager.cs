using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Core.DataManagement.V1.Protos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        readonly IStorageService _storageService;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;
        readonly IDataManagementService _dataManagementService;

        public MetadataManager(IStorageService storageService,
            IDataManagementService dataManagementService,
            ISettingsService settingsService,
            IMapper mapper)
        {
            _dataManagementService = dataManagementService;
            _storageService = storageService;
            _settingsService = settingsService;
            _mapper = mapper;
        }

        public async Task<List<KeyValuePairViewModel>> GetMetadata(User user, MetadataTypes type)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);

            switch (type)
            {
                case MetadataTypes.Sex:
                    return (await _storageService.GetJson<ListContainerViewModel>("SexTypes", 1, configurationContext)).Items;
                case MetadataTypes.ContentTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("ContentTypes", 1, configurationContext)).Items;
                case MetadataTypes.SourceTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("SourceTypes", 1, configurationContext)).Items;
                case MetadataTypes.SettingTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("SettingTypes", 1, configurationContext)).Items;
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        public async Task<List<SourceKeyValuePairViewModel>> GetSource(User user, MetadataTypes type)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);

            switch (type)
            {
                case MetadataTypes.SearchColumns:
                    return (await _storageService.GetJson<SourceListContainerViewModel>("SearchColumns", 1, configurationContext)).Items;
                default:
                    return new List<SourceKeyValuePairViewModel>();
            }
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

        public async Task<List<Department>> GetAllDepartments(User user)
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

        public async Task<List<ProcedureType>> GetProceduresByDepartment(Avalanche.Shared.Domain.Models.User user, int? departmentId)
        {
            await ValidateDepartmentsSupport(user, departmentId);

            var result = await _dataManagementService.GetProceduresByDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.GetProceduresByDepartmentRequest()
            {
                DepartmentId = departmentId
            });

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureType>>(result.ProcedureTypeList).ToList();
        }

        private async Task ValidateDepartmentsSupport(User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettings(configurationContext);

            if (!setupSettings.General.DepartmentsSupported)
            {
                throw new System.InvalidOperationException("Departments are not supported");
            }
        }

        private async Task ValidateDepartmentsSupport(User user, int? departmentId)
        {
            var configurationContext = _mapper.Map<User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettings(configurationContext);

#warning TODO: Check the strategy to throw business logic exceptions. Same exceptions in Patients Manager
            if (setupSettings.General.DepartmentsSupported)
            {
                if (departmentId == null || departmentId == 0)
                    throw new System.ArgumentNullException("Department value is invalid. It should not be null. Departments are supported.");
            }
            else
            {
                if (departmentId != null || departmentId != 0)
                    throw new System.ArgumentException("Department value is invalid. Departments are not supported.");
            }
        }
    }
}