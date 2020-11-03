using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
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

        public async Task<List<KeyValuePairViewModel>> GetMetadata(Avalanche.Shared.Domain.Models.User user, Shared.Domain.Enumerations.MetadataTypes type)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);

            switch (type)
            {
                case Shared.Domain.Enumerations.MetadataTypes.Sex:
                    return (await _storageService.GetJson<ListContainerViewModel>("SexTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.ContentTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("ContentTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.SourceTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("SourceTypes", 1, configurationContext)).Items;
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        public async Task<Department> AddDepartment(Avalanche.Shared.Domain.Models.User user, Department department)
        {
            await ValidateDepartmentsSupport(user);
            Preconditions.ThrowIfNull(nameof(department.Name), department.Name);

            var result = await _dataManagementService.AddDepartment(_mapper.Map<Department, AddDepartmentRequest>(department));
            return _mapper.Map<AddDepartmentResponse, Department>(result);
        }

        public async Task DeleteDepartment(Avalanche.Shared.Domain.Models.User user, string departmentName)
        {
            await ValidateDepartmentsSupport(user);
            Preconditions.ThrowIfNull(nameof(departmentName), departmentName);

            await _dataManagementService.DeleteDepartment(new DeleteDepartmentRequest() { DepartmentName = departmentName });
        }

        public async Task<List<Department>> GetAllDepartments(Avalanche.Shared.Domain.Models.User user)
        {
            await ValidateDepartmentsSupport(user);

            var result = await _dataManagementService.GetAllDepartments();

            return _mapper.Map<IList<DepartmentMessage>, IList<Department>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureType> AddProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType)
        {
            await ValidateDepartmentsSupport(user, procedureType.Department);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            var result = await _dataManagementService.AddProcedureType(_mapper.Map<ProcedureType, AddProcedureTypeRequest>(procedureType));
            return _mapper.Map<AddProcedureTypeResponse, ProcedureType>(result);
        }

        public async Task DeleteProcedureType(Avalanche.Shared.Domain.Models.User user, ProcedureType procedureType)
        {
            await ValidateDepartmentsSupport(user, procedureType.Department);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            await _dataManagementService.DeleteProcedureType(_mapper.Map<ProcedureType, DeleteProcedureTypeRequest>(procedureType));
        }

        public async Task<List<ProcedureType>> GetProceduresByDepartment(Avalanche.Shared.Domain.Models.User user, string departmentName = null)
        {
            await ValidateDepartmentsSupport(user, departmentName);

            var result = await _dataManagementService.GetProceduresByDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.GetProceduresByDepartmentRequest()
            {
                DepartmentName = departmentName
            });

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureType>>(result.ProcedureTypeList).ToList();
        }

        private async Task ValidateDepartmentsSupport(User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);

            if (!setupSettings.DepartmentsSupported)
            {
                throw new System.InvalidOperationException("Departments are not supported");
            }
        }

        private async Task ValidateDepartmentsSupport(User user, string departmentName)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);

#warning TODO: Check the strategy to throw business logic exceptions
            if (setupSettings.DepartmentsSupported)
            {
                if (string.IsNullOrEmpty(departmentName))
                    throw new System.ArgumentNullException("Department value is invalid. It should not be null.");
            }
            else 
            {
                if (!string.IsNullOrEmpty(departmentName))
                    throw new System.ArgumentException("Department value is invalid. Departments are not supported.");
            }                
        }
    }
}