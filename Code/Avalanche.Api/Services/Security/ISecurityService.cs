using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Services.Security
{
    public interface ISecurityService
    {
        Task<GetUsersResponse> GetAllUsers();
        Task<AddUserResponse> AddUserAsync(AddUserRequest request);
        Task<Empty> UpdateUserAsync(UpdateUserRequest request);
        Task<Empty> DeleteUserAsync(DeleteUserRequest request);
        Task<FindByUserNameResponse> FindByUserName(string userName);
    }
}
