using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ism.Common.Core.Aspects;
using Microsoft.Extensions.Logging;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.V1.Handlers
{
    public class SecurityServiceHandler : Client.V1.Protos.Security.SecurityBase
    {
        private readonly ILogger<SecurityServiceHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUsersManager _usersManager;

        public SecurityServiceHandler(ILogger<SecurityServiceHandler> logger, IMapper mapper, IUsersManager usersManager)
        {
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _usersManager = ThrowIfNullOrReturn(nameof(usersManager), usersManager);
        }

        [AspectLogger]
        public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNull(nameof(request.User), request.User);

            var response = await _usersManager.AddUser(_mapper.Map<NewUserModel>(request.User)).ConfigureAwait(false);
            var user = _mapper.Map<UserMessage>(response);

            return new AddUserResponse
            {
                User = user
            };
        }

        [AspectLogger]
        public override async Task<Empty> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNullOrDefault(nameof(request.UserId), request.UserId);

            _ = await _usersManager.DeleteUser(request.UserId).ConfigureAwait(false);
            return new Empty();
        }

        [AspectLogger]
        public override async Task<GetUsersResponse> GetUsers(Empty request, ServerCallContext context)
        {
            var response = await _usersManager.GetUsers().ConfigureAwait(false);
            var resultList = _mapper.Map<IList<UserMessage>>(response.ToList());

            return new GetUsersResponse() { Users = { resultList } };
        }

        [AspectLogger]
        public override async Task<Empty> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNull(nameof(request.Update), request.Update);

            await _usersManager.UpdateUser(_mapper.Map<UpdateUserModel>(request.Update)).ConfigureAwait(false);
            return new Empty();
        }

        [AspectLogger]
        public override async Task<Empty> UpdateUserPassword(UpdateUserPasswordRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNull(nameof(request.PasswordUpdate), request.PasswordUpdate);

            await _usersManager.UpdateUserPassword(_mapper.Map<UpdateUserPasswordModel>(request.PasswordUpdate)).ConfigureAwait(false);
            return new Empty();
        }

        [AspectLogger]
        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNullOrEmptyOrWhiteSpace(nameof(request.UserName), request.UserName);

            var response = await _usersManager.GetUser(request.UserName).ConfigureAwait(false);
            var user = _mapper.Map<UserMessage>(response);

            return new GetUserResponse
            {
                User = user
            };
        }

        [AspectLogger]
        public override async Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);

            var users = await _usersManager.SearchUsers(request.Keyword).ConfigureAwait(false);
            var response = new SearchUsersResponse();
            response.Users.Add(_mapper.Map<IList<UserModel>, IList<UserMessage>>(users.ToList()));

            return response;
        }

        [AspectLogger]
        public override async Task<VerifyUserLoginResponse> VerifyUserLogin(VerifyUserLoginRequest request, ServerCallContext context)
        {
            ThrowIfNull(nameof(request), request);
            ThrowIfNullOrEmptyOrWhiteSpace(nameof(request.UserName), request.UserName);

            var (loginValid, user) = await _usersManager.VerifyUserLogin(request.UserName, request.Password).ConfigureAwait(false);
            return new VerifyUserLoginResponse
            {
                LoginValid = loginValid,
                User = user == null ? null : _mapper.Map<UserModel, UserMessage>(user)
            };
        }
    }
}
