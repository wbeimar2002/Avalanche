using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> AddUser(UserModel user);
        Task AddOrUpdateUser(UserModel user);
        Task<int> DeleteUser(int userId);
        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel> FindByUserNameAsync(string userName);
    }
}
