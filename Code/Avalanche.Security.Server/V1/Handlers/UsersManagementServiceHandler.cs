using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Security.Server.Core.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.V1.Handlers
{
    public class UsersManagementServiceHandler : UsersManagement.UsersManagementBase
    {
        private readonly ILogger<UsersManagementServiceHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserRepository _UserRepository;

        public UsersManagementServiceHandler(ILogger<UsersManagementServiceHandler> logger, IMapper mapper, IUserRepository UserRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _UserRepository = UserRepository;
        }

        public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            return null;
        }

        public override async Task<Empty> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            return null;
        }

        public override async Task<GetUsersResponse> GetUsers(Empty request, ServerCallContext context)
        {
            return null;
        }

        public override async Task<Empty> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            return null;
        }
    }
}
