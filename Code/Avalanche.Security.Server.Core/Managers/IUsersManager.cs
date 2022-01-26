using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Managers
{
    public interface IUsersManager
    {
        Task<UserModel> AddUser(UserModel user);
        Task UpdateUser(UserModel user);
        Task<int> DeleteUser(int userId);
        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel> FindByUserNameAsync(string userName);
    }
}