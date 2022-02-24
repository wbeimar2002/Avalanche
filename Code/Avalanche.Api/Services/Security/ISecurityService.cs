using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public interface ISecurityService
    {
        Task<AddUserResponse> AddUser(AddUserRequest request);

        Task DeleteUser(DeleteUserRequest request);

        Task<GetUsersResponse> GetAllUsers();
        Task<GetUserResponse> GetUser(string userName);

        Task<SearchUsersResponse> SearchUsers(string keyword);

        Task UpdateUser(UpdateUserRequest request);
        Task UpdateUserPassword(UpdateUserPasswordRequest request);
        Task<VerifyUserLoginResponse> VerifyUserLogin(string userName, string password);
    }
}
