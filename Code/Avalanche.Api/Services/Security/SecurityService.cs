using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly SecurityServiceSecureClient _client;

        public SecurityService(SecurityServiceSecureClient client) => _client = client;

        public async Task<GetUsersResponse> GetAllUsers() =>
            await _client.GetUsers().ConfigureAwait(false);

        public async Task<GetUserResponse> GetUser(string userName) =>
            await _client.GetUser(
                new GetUserRequest
                {
                    UserName = userName
                })
            .ConfigureAwait(false);

        public async Task<AddUserResponse> AddUser(AddUserRequest request) =>
           await _client.AddUser(request).ConfigureAwait(false);

        public async Task<Empty> UpdateUser(UpdateUserRequest request) =>
           await _client.UpdateUser(request).ConfigureAwait(false);

        public async Task<Empty> DeleteUser(DeleteUserRequest request) =>
           await _client.DeleteUser(request).ConfigureAwait(false);

        public async Task<SearchUsersResponse> SearchUsers(string keyword) =>
            await _client.SearchUsers(new SearchUsersRequest()
            {
                Keyword = keyword
            }).ConfigureAwait(false);
    }
}
