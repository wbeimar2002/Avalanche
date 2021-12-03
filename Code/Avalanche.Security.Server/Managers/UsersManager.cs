using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Managers
{
    public class UsersManager : IUsersManager
    {
        public Task AddOrUpdateUser(UserModel user) => throw new System.NotImplementedException();
        public Task<UserModel> AddUser(UserModel user) => throw new System.NotImplementedException();
        public Task<int> DeleteUser(int userId) => throw new System.NotImplementedException();
        public Task<UserModel> FindByUserNameAsync(string userName) => throw new System.NotImplementedException();
        public Task<IEnumerable<UserModel>> GetAllUsers() => throw new System.NotImplementedException();
    }
}
