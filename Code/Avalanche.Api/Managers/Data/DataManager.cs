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
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Data
{
    public abstract class DataManager : IDataManager
    {
        private readonly IDataManagementService _dataManagementService;
        private readonly IStorageService _storageService;
        private readonly ISecurityService _securityService;

        private readonly IMapper _mapper;
        private readonly ConfigurationContext _configurationContext;

        private readonly SetupConfiguration _setupConfiguration;

        public DataManager(
            IMapper mapper,
            IDataManagementService dataManagementService,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor,
            SetupConfiguration setupConfiguration,
            ISecurityService securityService)
        {
            _dataManagementService = dataManagementService;
            _storageService = storageService;
            _mapper = mapper;
            _setupConfiguration = setupConfiguration;
            _securityService = securityService;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
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
            Preconditions.ThrowIfNull(nameof(user.LastName), user.LastName);
            Preconditions.ThrowIfNull(nameof(user.UserName), user.UserName);
            Preconditions.ThrowIfNull(nameof(user.Password), user.Password);

            var request = new AddUserRequest()
            {
                User = _mapper.Map<NewUserMessage>(user)
            };

            var result = await _securityService.AddUser(request).ConfigureAwait(false);

            return _mapper.Map<UserModel>(result.User);
        }

        public async Task UpdateUser(UserModel user)
        {
            Preconditions.ThrowIfNull(nameof(user.Id), user.Id);
            Preconditions.ThrowIfNull(nameof(user.FirstName), user.FirstName);
            Preconditions.ThrowIfNull(nameof(user.LastName), user.LastName);
            Preconditions.ThrowIfNull(nameof(user.UserName), user.UserName);

            // If password is not null then it's a password Update otherwise the update was just for other User information 
            if (user.Password != null)
            {
                var request = new UpdateUserPasswordRequest()
                {
                    PasswordUpdate = _mapper.Map<UpdateUserPasswordMessage>(user)
                };

                await _securityService.UpdateUserPassword(request).ConfigureAwait(false);
            }
            else
            {
                var request = new UpdateUserRequest()
                {
                    Update = _mapper.Map<UpdateUserMessage>(user)
                };

                await _securityService.UpdateUser(request).ConfigureAwait(false);
            }
        }

        public async Task DeleteUser(int userId)
        {
            Preconditions.ThrowIfNull(nameof(userId), userId);
            await _securityService.DeleteUser(new DeleteUserRequest() { UserId = userId }).ConfigureAwait(false);
        }

        public async Task<IList<UserModel>> GetAllUsers()
        {
            var result = await _securityService.GetAllUsers().ConfigureAwait(false);

            return _mapper.Map<IList<UserMessage>, IList<UserModel>>(result.Users)
                .OrderBy(d => d.LastName).ToList();
        }

        public async Task<DepartmentModel> AddDepartment(DepartmentModel department)
        {
            Preconditions.ThrowIfNull(nameof(department.Name), department.Name);

            var result = await _dataManagementService.AddDepartment(_mapper.Map<DepartmentModel, AddDepartmentRequest>(department)).ConfigureAwait(false);
            return _mapper.Map<AddDepartmentResponse, DepartmentModel>(result);
        }

        public async Task DeleteDepartment(int departmentId)
        {
            await _dataManagementService.DeleteDepartment(new DeleteDepartmentRequest() { DepartmentId = departmentId }).ConfigureAwait(false);
        }

        public async Task<IList<DepartmentModel>> GetAllDepartments()
        {
            var result = await _dataManagementService.GetAllDepartments().ConfigureAwait(false);

            return _mapper.Map<IList<DepartmentMessage>, IList<DepartmentModel>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureTypeModel> AddProcedureType(ProcedureTypeModel procedureType)
        {
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);
            Preconditions.ThrowIfNull(nameof(procedureType.DepartmentId), procedureType.DepartmentId);

            var result = await _dataManagementService.AddProcedureType(_mapper.Map<ProcedureTypeModel, AddProcedureTypeRequest>(procedureType)).ConfigureAwait(false);
            return _mapper.Map<AddProcedureTypeResponse, ProcedureTypeModel>(result);
        }

        public async Task DeleteProcedureType(ProcedureTypeModel procedureType)
        {
            Preconditions.ThrowIfNull(nameof(procedureType.Name), procedureType.Name);
            Preconditions.ThrowIfNull(nameof(procedureType.DepartmentId), procedureType.DepartmentId);

            await _dataManagementService.DeleteProcedureType(_mapper.Map<ProcedureTypeModel, DeleteProcedureTypeRequest>(procedureType)).ConfigureAwait(false);
        }

        public async Task<List<ProcedureTypeModel>> GetProcedureTypesByDepartment(int? departmentId)
        {
            Preconditions.ThrowIfNull(nameof(departmentId), departmentId);

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

        public abstract Task<IList<AliasIndexModel>> GetGpioPins();
    }
}
