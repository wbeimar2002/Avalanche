using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Managers
{
    // 
    /// <summary>
    /// This is currently nothing but a proxy/not a very valuable abstraction.
    /// Keeping it around for now in case we need to wrap anymore business logic around these methods in the future.
    /// e.g. Notify VSS if local user Added or Updated
    /// </summary>
    public class UsersManager : IUsersManager
    {
        private readonly IUserRepository _userRepository;

        public UsersManager(IUserRepository userRepository) =>
            _userRepository = userRepository;

        public async Task<UserModel> AddUser(NewUserModel user) =>
            await _userRepository.AddUser(user).ConfigureAwait(false);

        public async Task<int> DeleteUser(int userId) =>
            await _userRepository.DeleteUser(userId).ConfigureAwait(false);

        public async Task<UserModel?> GetUser(string userName) =>
            await _userRepository.GetUser(userName).ConfigureAwait(false);

        public async Task<IEnumerable<UserModel>> GetUsers() =>
                    await _userRepository.GetUsers().ConfigureAwait(false);
        public async Task<IEnumerable<UserModel>> SearchUsers(string keyword) =>
            await _userRepository.SearchUsers(keyword).ConfigureAwait(false);

        public async Task UpdateUser(UpdateUserModel user) =>
            await _userRepository.UpdateUser(user).ConfigureAwait(false);
    }
}
