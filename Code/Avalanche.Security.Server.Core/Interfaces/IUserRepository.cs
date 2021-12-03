using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> AddUser(UserModel User);
        Task AddOrUpdateUser(UserModel User);
        Task<int> DeleteUser(int UserId);
        Task<IEnumerable<UserModel>> GetAllUsers();
    }
}
