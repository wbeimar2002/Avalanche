using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Data
{
    public class DataManager : IDataManager
    {
        private readonly IDataManagementService _dataManagementService;

        private readonly IMapper _mapper;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly SetupConfiguration _setupConfiguration;

        public DataManager(
            IMapper mapper,
            IDataManagementService dataManagementService,
            IHttpContextAccessor httpContextAccessor,
            SetupConfiguration setupConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataManagementService = dataManagementService;
            _mapper = mapper;
            _setupConfiguration = setupConfiguration;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<DepartmentModel> AddDepartment(DepartmentModel department)
        {
            await ValidateDepartmentsSupport();
            Preconditions.ThrowIfNull(nameof(department.Name), department.Name);

            var result = await _dataManagementService.AddDepartment(_mapper.Map<DepartmentModel, AddDepartmentRequest>(department));
            return _mapper.Map<AddDepartmentResponse, DepartmentModel>(result);
        }

        public async Task DeleteDepartment(int departmentId)
        {
            await ValidateDepartmentsSupport();

            await _dataManagementService.DeleteDepartment(new DeleteDepartmentRequest() { DepartmentId = departmentId });
        }

        public async Task<IList<DepartmentModel>> GetAllDepartments()
        {
            await ValidateDepartmentsSupport();

            var result = await _dataManagementService.GetAllDepartments();

            return _mapper.Map<IList<DepartmentMessage>, IList<DepartmentModel>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureTypeModel> AddProcedureType(ProcedureTypeModel procedureType)
        {
            await ValidateDepartmentsSupport(procedureType.DepartmentId);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            var result = await _dataManagementService.AddProcedureType(_mapper.Map<ProcedureTypeModel, AddProcedureTypeRequest>(procedureType));
            return _mapper.Map<AddProcedureTypeResponse, ProcedureTypeModel>(result);
        }

        public async Task DeleteProcedureType(ProcedureTypeModel procedureType)
        {
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            await ValidateDepartmentsSupport(procedureType.DepartmentId);

            await _dataManagementService.DeleteProcedureType(_mapper.Map<ProcedureTypeModel, DeleteProcedureTypeRequest>(procedureType));
        }

        public async Task<List<ProcedureTypeModel>> GetProcedureTypesByDepartment(int? departmentId)
        {
            await ValidateDepartmentsSupport(departmentId);

            var result = await _dataManagementService.GetProcedureTypesByDepartment(new Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypesByDepartmentRequest()
            {
                DepartmentId = departmentId
            });

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureTypeModel>>(result.ProcedureTypeList).ToList();
        }

        public async Task<List<ProcedureTypeModel>> GetAllProcedureTypes()
        {
            var result = await _dataManagementService.GetAllProcedureTypes();
            var list = _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureTypeModel>>(result.ProcedureTypeList).ToList();

            return list;
        }

        public async Task ValidateDepartmentsSupport()
        {
            bool departmentSupported = _setupConfiguration.General.DepartmentsSupported;
            if (!departmentSupported)
            {
                throw new System.InvalidOperationException("Departments are not supported");
            }
        }

        public async Task ValidateDepartmentsSupport(int? departmentId)
        {
            bool departmentSupported = _setupConfiguration.General.DepartmentsSupported;
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

        public async Task<LabelModel> AddLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            var result = await _dataManagementService.AddLabel(_mapper.Map<LabelModel, AddLabelRequest>(label));
            return _mapper.Map<AddLabelResponse, LabelModel>(result);
        }

        public async Task UpdateLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Id), label.Id);
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);

            await _dataManagementService.UpdateLabel(_mapper.Map<LabelModel, UpdateLabelRequest>(label));
        }

        public async Task DeleteLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            await _dataManagementService.DeleteLabel(_mapper.Map<LabelModel, DeleteLabelRequest>(label));
        }

        public async Task<List<LabelModel>> GetLabelsByProcedureType(int? procedureTypeId)
        {
            if (procedureTypeId != null && procedureTypeId <= 0)
            {
                procedureTypeId = null;
            }

            var result = await _dataManagementService.GetLabelsByProcedureType(new GetLabelsByProcedureTypeRequest()
            {
                ProcedureTypeId = procedureTypeId
            });

            return _mapper.Map<IList<LabelMessage>, IList<LabelModel>>(result.LabelList).ToList();
        }

        public async Task<List<LabelModel>> GetAllLabels()
        {
            var result = await _dataManagementService.GetAllLabels();

            return _mapper.Map<IList<LabelMessage>, IList<LabelModel>>(result.LabelList).ToList();
        }

        public async Task<LabelModel> GetLabel(string labelName, int? procedureTypeId)
        {
            if (procedureTypeId != null && procedureTypeId <= 0)
            {
                procedureTypeId = null;
            }

            var result = await _dataManagementService.GetLabel(new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelRequest()
            {
                LabelName = labelName,
                ProcedureTypeId = procedureTypeId,                
                IgnoreCustomExceptions = true
            });

            return _mapper.Map<LabelMessage, LabelModel>(result);
        }
    }
}
