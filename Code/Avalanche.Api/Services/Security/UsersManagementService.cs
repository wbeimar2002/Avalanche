using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public class UsersManagementService : IUsersManagementService
    {
        private readonly UsersManagementServiceClient _usersManagementServiceClient;

        public UsersManagementService(UsersManagementServiceClient usersManagementServiceClient)
        {
            _usersManagementServiceClient = usersManagementServiceClient;
        }

        public async Task<GetUsersResponse> GetAllUsers() =>
            await _usersManagementServiceClient.GetUsersAsync(new Empty()).ConfigureAwait(false);

        public async Task<AddUserResponse> AddUserAsync(AddUserRequest request) =>
           await _usersManagementServiceClient.AddUserAsync(request);

        public async Task<Empty> UpdateUserAsync(UpdateUserRequest request) =>
           await _usersManagementServiceClient.UpdateUserAsync(request);

        public async Task<Empty> DeleteUserAsync(DeleteUserRequest request) =>
           await _usersManagementServiceClient.DeleteUserAsync(request);
    }
}
