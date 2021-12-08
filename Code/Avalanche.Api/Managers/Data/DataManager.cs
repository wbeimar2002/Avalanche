using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Security.Server.Client.V1.Protos;
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
        private readonly IStorageService _storageService;
        private readonly IUsersManagementService _usersManagementService;

        private readonly IMapper _mapper;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;

        private readonly SetupConfiguration _setupConfiguration;

        public DataManager(
            IMapper mapper,
            IDataManagementService dataManagementService,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor,
            SetupConfiguration setupConfiguration,
            IUsersManagementService usersManagementService)
        {
            _dataManagementService = dataManagementService;
            _storageService = storageService;
            _mapper = mapper;
            _setupConfiguration = setupConfiguration;
            _usersManagementService = usersManagementService;

            _user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<List<dynamic>> GetList(string sourceKey, string jsonKey = null)
        {
            if (jsonKey == null)
            {
                return await _storageService.GetJsonDynamicList(sourceKey, 1, _configurationContext).ConfigureAwait(false);
            }
            else
            {
                var settingValues = await _storageService.GetJson(sourceKey, 1, _configurationContext).ConfigureAwait(false);
                return DynamicSettingsHelper.GetEmbeddedList(jsonKey, settingValues);
            }
        }

        public async Task<UserModel> AddUser(UserModel user)
        {
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.FirstName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.LastName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.UserName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.Password);

            var result = await _usersManagementService.AddUserAsync(_mapper.Map<UserModel, AddUserRequest>(user)).ConfigureAwait(false);
            return _mapper.Map<AddUserResponse, UserModel>(result);
        }

        public async Task UpdateUser(UserModel user)
        {
            Preconditions.ThrowIfNull(nameof(user.Id), user.Id);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.FirstName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.LastName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.UserName);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.Password);

            await _usersManagementService.UpdateUserAsync(_mapper.Map<UserModel, UpdateUserRequest>(user)).ConfigureAwait(false);
        }

        public async Task DeleteUser(int userId)
        {
            Preconditions.ThrowIfNull(nameof(userId), userId);
            await _usersManagementService.DeleteUserAsync(new DeleteUserRequest() { UserId = userId }).ConfigureAwait(false);
        }

        public async Task<IList<UserModel>> GetAllUsers()
        {
            var result = await _usersManagementService.GetAllUsers().ConfigureAwait(false);

            return _mapper.Map<IList<UserMessage>, IList<UserModel>>(result.Users)
                .OrderBy(d => d.LastName).ToList();
        }

        public async Task<DepartmentModel> AddDepartment(DepartmentModel department)
        {
            await ValidateDepartmentsSupport().ConfigureAwait(false);
            Preconditions.ThrowIfNull(nameof(department.Name), department.Name);

            var result = await _dataManagementService.AddDepartment(_mapper.Map<DepartmentModel, AddDepartmentRequest>(department)).ConfigureAwait(false);
            return _mapper.Map<AddDepartmentResponse, DepartmentModel>(result);
        }

        public async Task DeleteDepartment(int departmentId)
        {
            await ValidateDepartmentsSupport().ConfigureAwait(false);

            await _dataManagementService.DeleteDepartment(new DeleteDepartmentRequest() { DepartmentId = departmentId }).ConfigureAwait(false);
        }

        public async Task<IList<DepartmentModel>> GetAllDepartments()
        {
            await ValidateDepartmentsSupport().ConfigureAwait(false);

            var result = await _dataManagementService.GetAllDepartments().ConfigureAwait(false);

            return _mapper.Map<IList<DepartmentMessage>, IList<DepartmentModel>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureTypeModel> AddProcedureType(ProcedureTypeModel procedureType)
        {
            await ValidateDepartmentsSupport(procedureType.DepartmentId).ConfigureAwait(false);
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            var result = await _dataManagementService.AddProcedureType(_mapper.Map<ProcedureTypeModel, AddProcedureTypeRequest>(procedureType)).ConfigureAwait(false);
            return _mapper.Map<AddProcedureTypeResponse, ProcedureTypeModel>(result);
        }

        public async Task DeleteProcedureType(ProcedureTypeModel procedureType)
        {
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);

            await ValidateDepartmentsSupport(procedureType.DepartmentId).ConfigureAwait(false);

            await _dataManagementService.DeleteProcedureType(_mapper.Map<ProcedureTypeModel, DeleteProcedureTypeRequest>(procedureType)).ConfigureAwait(false);
        }

        public async Task<List<ProcedureTypeModel>> GetProcedureTypesByDepartment(int? departmentId)
        {
            await ValidateDepartmentsSupport(departmentId);

            var result = await _dataManagementService.GetProcedureTypesByDepartment(new GetProcedureTypesByDepartmentRequest()
            {
                DepartmentId = departmentId
            }).ConfigureAwait(false);

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureTypeModel>>(result.ProcedureTypeList).ToList();
        }

        public async Task<List<ProcedureTypeModel>> GetAllProcedureTypes()
        {
            var result = await _dataManagementService.GetAllProcedureTypes().ConfigureAwait(false);
            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureTypeModel>>(result.ProcedureTypeList).ToList();
        }

        public async Task ValidateDepartmentsSupport()
        {
            var departmentSupported = _setupConfiguration.General.DepartmentsSupported;
            if (!departmentSupported)
            {
                throw new InvalidOperationException("Departments are not supported");
            }
        }

        public async Task ValidateDepartmentsSupport(int? departmentId)
        {
            var departmentSupported = _setupConfiguration.General.DepartmentsSupported;
#warning TODO: Check the strategy to throw business logic exceptions. Same exceptions in Patients Manager
            if (departmentSupported)
            {
                if (departmentId == null || departmentId == 0)
                {
                    throw new ArgumentNullException("Department value is invalid. It should not be null. Departments are supported.");
                }
            }
            else
            {
                if (departmentId != null && departmentId != 0)
                {
                    throw new ArgumentException("Department value is invalid. Departments are not supported.");
                }
            }
        }

        public async Task<LabelModel> AddLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            var result = await _dataManagementService.AddLabel(_mapper.Map<LabelModel, AddLabelRequest>(label)).ConfigureAwait(false);
            return _mapper.Map<AddLabelResponse, LabelModel>(result);
        }

        public async Task UpdateLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Id), label.Id);
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);

            await _dataManagementService.UpdateLabel(_mapper.Map<LabelModel, UpdateLabelRequest>(label)).ConfigureAwait(false);
        }

        public async Task DeleteLabel(LabelModel label)
        {
            Preconditions.ThrowIfNull(nameof(label.Name), label.Name);
            await _dataManagementService.DeleteLabel(_mapper.Map<LabelModel, DeleteLabelRequest>(label)).ConfigureAwait(false);
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
            }).ConfigureAwait(false);

            return _mapper.Map<IList<LabelMessage>, IList<LabelModel>>(result.LabelList).ToList();
        }

        public async Task<List<LabelModel>> GetAllLabels()
        {
            var result = await _dataManagementService.GetAllLabels().ConfigureAwait(false);

            return _mapper.Map<IList<LabelMessage>, IList<LabelModel>>(result.LabelList).ToList();
        }

        public async Task<LabelModel> GetLabel(string labelName, int? procedureTypeId)
        {
            if (procedureTypeId != null && procedureTypeId <= 0)
            {
                procedureTypeId = null;
            }

            var result = await _dataManagementService.GetLabel(new GetLabelRequest()
            {
                LabelName = labelName,
                ProcedureTypeId = procedureTypeId,
                IgnoreCustomExceptions = true
            }).ConfigureAwait(false);

            return _mapper.Map<LabelMessage, LabelModel>(result);
        }
    }
}
