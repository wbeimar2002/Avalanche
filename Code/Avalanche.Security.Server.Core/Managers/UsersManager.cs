using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Shared.Infrastructure.Security.Hashing;

namespace Avalanche.Security.Server.Core.Managers
{
    // 
    /// <summary>
    /// This is currently nothing but a proxy/not a very valuable abstraction.
    /// Keeping it around for now in case we need to wrap anymore business logic around these methods in the future.
    /// e.g. Notify VSS if local user Added or Updated
    /// There is also a reasonable argument that the public methods on UserRepository, which take models could be moved here
    /// and the private Repository methods, which take Entities, could be lifted up and made public.
    /// </summary>
    public class UsersManager : IUsersManager
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepository _userRepository;

        public UsersManager(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public Task<UserModel> AddUser(NewUserModel user) =>
            _userRepository.AddUser(user);

        public Task<int> DeleteUser(int userId) =>
            _userRepository.DeleteUser(userId);

        public Task<UserModel?> GetUser(string userName) =>
            _userRepository.GetUser(userName);

        public Task<IEnumerable<UserModel>> GetUsers() =>
            _userRepository.GetUsers();

        public Task<IEnumerable<UserModel>> SearchUsers(string keyword) =>
            _userRepository.SearchUsers(keyword);

        public Task UpdateUser(UpdateUserModel update) =>
            _userRepository.UpdateUser(update);

        public Task UpdateUserPassword(UpdateUserPasswordModel passwordUpdate) =>
            _userRepository.UpdateUserPassword(passwordUpdate);

        public async Task<(bool LoginValid, UserModel? User)> VerifyUserLogin(string userName, string password)
        {
            var user = await _userRepository.GetUser(userName).ConfigureAwait(false);
            if (user == null)
            {
                return (false, null);
            }

            var valid = _passwordHasher.PasswordMatches(password, user.PasswordHash);
            if (valid)
            {
                return (valid, user);
            }

            // Make sure to return null User if password doesn't match
            return (false, null);
        }

        public async Task<(bool LoginValid, UserModel? User)> VerifyAdminUserLogin(string userName, string password)
        {
            var (loginValid, user) = await VerifyUserLogin(userName, password).ConfigureAwait(false);

            if (!loginValid || user is null)
            {
                return (loginValid, user);
            }

            return (user.IsAdmin, user);
        }
    }
}
