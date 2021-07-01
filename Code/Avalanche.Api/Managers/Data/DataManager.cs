using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;

using Ism.Common.Core.Configuration.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Data
{
    public class DataManager : IDataManager
    {
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;
        private readonly IDataManagementService _dataManagementService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        readonly UserModel user;
        readonly ConfigurationContext configurationContext;

        public DataManager(IStorageService storageService,
            IDataManagementService dataManagementService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataManagementService = dataManagementService;
            _storageService = storageService;
            _mapper = mapper;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<List<dynamic>> GetList(string sourceKey, string jsonKey = null)
        {
            if (jsonKey == null)
            {
                return await _storageService.GetJsonDynamicList(sourceKey, 1, configurationContext);
            }
            else
            {
                var settingValues = await _storageService.GetJson(sourceKey, 1, configurationContext);
                return SettingsHelper.GetEmbeddedList(jsonKey, settingValues);
            }
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

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureTypeModel>>(result.ProcedureTypeList).ToList();
        }

        public async Task ValidateDepartmentsSupport()
        {
            var setupSettings = await _storageService.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, configurationContext);

            bool departmentSupported = setupSettings.General.DepartmentsSupported;
            if (!departmentSupported)
            {
                throw new System.InvalidOperationException("Departments are not supported");
            }
        }

        public async Task ValidateDepartmentsSupport(int? departmentId)
        {
            var setupSettings = await _storageService.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, configurationContext);

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

        public async Task<LabelModel> AddLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            var result = await _dataManagementService.AddLabel(_mapper.Map<LabelModel, AddLabelRequest>(label));
            return _mapper.Map<AddLabelResponse, LabelModel>(result);
        }

        public async Task DeleteLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            await _dataManagementService.DeleteLabel(_mapper.Map<LabelModel, DeleteLabelRequest>(label));
        }

        public async Task<List<LabelModel>> GetLabelsByProcedureType(int? procedureTypeId)
        {
            var result = await _dataManagementService.GetLabelsByProcedureType(new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelsByProcedureTypeRequest()
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
    }
}