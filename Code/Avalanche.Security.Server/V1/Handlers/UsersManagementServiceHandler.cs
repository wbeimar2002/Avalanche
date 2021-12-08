using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Managers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Avalanche.Security.Server.V1.Handlers
{
    public class UsersManagementServiceHandler : UsersManagement.UsersManagementBase
    {
        private readonly ILogger<UsersManagementServiceHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUsersManager _usersManager;

        public UsersManagementServiceHandler(ILogger<UsersManagementServiceHandler> logger, IMapper mapper, IUsersManager usersManager)
        {
            _logger = logger;
            _mapper = mapper;
            _usersManager = usersManager;
        }

        public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            var response = await _usersManager.AddUser(_mapper.Map<UserModel>(request));
            return _mapper.Map<AddUserResponse>(response);
        }

        public override async Task<Empty> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var response = await _usersManager.DeleteUser(request.UserId);
            return new Empty();
        }

        public override async Task<GetUsersResponse> GetUsers(Empty request, ServerCallContext context)
        {
            var response = await _usersManager.GetAllUsers();
            return _mapper.Map<GetUsersResponse>(response);
        }

        public override async Task<Empty> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            await _usersManager.UpdateUser(_mapper.Map<UserModel>(request));
            return new Empty();
        }

        public override async Task<FindByUserNameResponse> FindByUserName(FindByUserNameRequest request, ServerCallContext context)
        {
            var response = _usersManager.FindByUserNameAsync(request.UserName);
            return _mapper.Map<FindByUserNameResponse>(response);
        }
    }
}
