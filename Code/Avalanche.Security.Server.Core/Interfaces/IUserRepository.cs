using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> AddUser(NewUserModel user);
        Task UpdateUser(UpdateUserModel user);
        Task<int> DeleteUser(int userId);
        Task<IEnumerable<UserModel>> GetUsers();
        Task<UserModel?> GetUser(string userName);
        Task<IEnumerable<UserModel>> SearchUsers(string keyword);
    }
}
