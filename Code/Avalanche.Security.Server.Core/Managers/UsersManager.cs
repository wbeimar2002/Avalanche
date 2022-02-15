using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.Core.Managers
{
    public class UsersManager : IUsersManager
    {
        //TODO: Finish the preconditions
        private readonly IUserRepository _userRepository;

        public UsersManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task UpdateUser(UserModel user)
        {
            ThrowIfNull(nameof(user), user);
            ThrowIfNullOrEmpty(nameof(user), user.UserName);
            ThrowIfTrue(nameof(user), user.UserName.Length > 64);

            await _userRepository.AddOrUpdateUser(user);
        }

        public async Task<UserModel> AddUser(UserModel user)
        {
            ThrowIfNull(nameof(user), user);
            ThrowIfNullOrEmpty(nameof(user), user.UserName);
            ThrowIfTrue(nameof(user), user.UserName.Length > 64);

            return await _userRepository.AddUser(user);
        }

        public async Task<int> DeleteUser(int userId)
        {
            return await _userRepository.DeleteUser(userId);
        }

        public async Task<UserModel> FindByUserNameAsync(string userName)
        {
            return await _userRepository.FindByUserNameAsync(userName);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<IEnumerable<UserModel>> SearchUsers(string keyword)
        {
            return await _userRepository.SearchUsers(keyword);
        }
    }
}
