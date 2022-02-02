using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly SecurityServiceClient _SecurityServiceClient;

        public SecurityService(SecurityServiceClient SecurityServiceClient)
        {
            _SecurityServiceClient = SecurityServiceClient;
        }

        public async Task<GetUsersResponse> GetAllUsers() =>
            await _SecurityServiceClient.GetUsersAsync(new Empty()).ConfigureAwait(false);

        public async Task<FindByUserNameResponse> FindByUserName(string userName) =>
            await _SecurityServiceClient.FindByUserNameAsync(new FindByUserNameRequest()
            {
                UserName = userName
            }).ConfigureAwait(false);

        public async Task<AddUserResponse> AddUserAsync(AddUserRequest request) =>
           await _SecurityServiceClient.AddUserAsync(request);

        public async Task<Empty> UpdateUserAsync(UpdateUserRequest request) =>
           await _SecurityServiceClient.UpdateUserAsync(request);

        public async Task<Empty> DeleteUserAsync(DeleteUserRequest request) =>
           await _SecurityServiceClient.DeleteUserAsync(request);

        public async Task<SearchUsersResponse> SearchUsers(string keyword) =>
            await _SecurityServiceClient.SearchUsersAsync(new SearchUsersRequest()
            {
                Keyword = keyword
            }).ConfigureAwait(false);
    }
}
