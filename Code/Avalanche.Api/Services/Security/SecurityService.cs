using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1;
using Avalanche.Security.Server.Client.V1.Protos;

namespace Avalanche.Api.Services.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly SecurityServiceSecureClient _client;

        public SecurityService(SecurityServiceSecureClient client) => _client = client;

        public Task<GetUsersResponse> GetAllUsers() =>
            _client.GetUsers();

        public Task<GetUserResponse> GetUser(string userName) =>
            _client.GetUser(
                new GetUserRequest
                {
                    UserName = userName
                });

        public Task<AddUserResponse> AddUser(AddUserRequest request) =>
           _client.AddUser(request);

        public Task UpdateUser(UpdateUserRequest request) =>
           _client.UpdateUser(request);

        public Task DeleteUser(DeleteUserRequest request) =>
           _client.DeleteUser(request);

        public Task<SearchUsersResponse> SearchUsers(string keyword) =>
        _client.SearchUsers(new SearchUsersRequest()
        {
            Keyword = keyword
        });

        public async Task<VerifyUserLoginResponse> VerifyUserLogin(string userName, string password) =>
            await _client.VerifyUserLogin(
                new VerifyUserLoginRequest
                {
                    UserName = userName,
                    Password = password
                }).ConfigureAwait(false);

        public Task UpdateUserPassword(UpdateUserPasswordRequest request) =>
            _client.UpdateUserPassword(request);
    }
}
