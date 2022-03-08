using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Interfaces
{
    public interface IUsersManager
    {
        Task<UserModel> AddUser(NewUserModel user);

        Task<int> DeleteUser(int userId);

        Task<UserModel?> GetUser(string userName);

        Task<IEnumerable<UserModel>> GetUsers();

        Task<IEnumerable<UserModel>> SearchUsers(string keyword);

        Task UpdateUser(UpdateUserModel update);

        Task UpdateUserPassword(UpdateUserPasswordModel passwordUpdate);

        Task<(bool LoginValid, UserModel? User)> VerifyUserLogin(string userName, string password);
    }
}
