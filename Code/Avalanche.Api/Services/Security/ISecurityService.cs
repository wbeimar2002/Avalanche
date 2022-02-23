using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public interface ISecurityService
    {
        Task<GetUsersResponse> GetAllUsers();
        Task<AddUserResponse> AddUser(AddUserRequest request);
        Task<Empty> UpdateUser(UpdateUserRequest request);
        Task<Empty> DeleteUser(DeleteUserRequest request);
        Task<GetUserResponse> GetUser(string userName);
        Task<SearchUsersResponse> SearchUsers(string keyword);
    }
}
