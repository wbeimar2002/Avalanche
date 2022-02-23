using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly SecurityServiceClient _client;

        public SecurityService(SecurityServiceClient client) => _client = client;

        public async Task<GetUsersResponse> GetAllUsers() =>
            await _client.GetUsersAsync(new Empty()).ConfigureAwait(false);

        public async Task<FindByUserNameResponse> FindByUserName(string userName) =>
            await _client.FindByUserNameAsync(new FindByUserNameRequest()
            {
                UserName = userName
            }).ConfigureAwait(false);

        public async Task<AddUserResponse> AddUser(AddUserRequest request) =>
           await _client.AddUserAsync(request).ConfigureAwait(false);

        public async Task<Empty> UpdateUser(UpdateUserRequest request) =>
           await _client.UpdateUserAsync(request).ConfigureAwait(false);

        public async Task<Empty> DeleteUser(DeleteUserRequest request) =>
           await _client.DeleteUserAsync(request).ConfigureAwait(false);

        public async Task<SearchUsersResponse> SearchUsers(string keyword) =>
            await _client.SearchUsersAsync(new SearchUsersRequest()
            {
                Keyword = keyword
            }).ConfigureAwait(false);
    }
}
