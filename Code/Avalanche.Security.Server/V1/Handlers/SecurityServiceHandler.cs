using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Managers;
using Avalanche.Security.Server.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Avalanche.Security.Server.V1.Handlers
{
    public class SecurityServiceHandler : Client.V1.Protos.Security.SecurityBase
    {
        private readonly ILogger<SecurityServiceHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUsersManager _usersManager;

        public SecurityServiceHandler(ILogger<SecurityServiceHandler> logger, IMapper mapper, IUsersManager usersManager)
        {
            _logger = logger;
            _mapper = mapper;
            _usersManager = usersManager;
        }

        public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            var response = await _usersManager.AddUser(_mapper.Map<UserModel>(request.User));

            var user = _mapper.Map<UserMessage>(response);

            var result = new AddUserResponse();
            result.User = user;
            return result;
        }

        public override async Task<Empty> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var response = await _usersManager.DeleteUser(request.UserId);
            return new Empty();
        }

        public override async Task<GetUsersResponse> GetUsers(Empty request, ServerCallContext context)
        {
            var response = await _usersManager.GetAllUsers();

            var resultList = _mapper.Map<IList<UserMessage>>(response.ToList());

            var result = new GetUsersResponse();
            result.Users.Add(resultList);
            return result;
        }

        public override async Task<Empty> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            await _usersManager.UpdateUser(_mapper.Map<UserModel>(request.User));
            return new Empty();
        }

        public override async Task<FindByUserNameResponse> FindByUserName(FindByUserNameRequest request, ServerCallContext context)
        {
            var response = await _usersManager.FindByUserNameAsync(request.UserName);

            var user = _mapper.Map<UserMessage>(response);

            var result = new FindByUserNameResponse();
            result.User = user;
            return result;
        }
    }
}
