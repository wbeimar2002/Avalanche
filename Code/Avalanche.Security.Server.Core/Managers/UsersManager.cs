using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;

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
            await _userRepository.AddOrUpdateUser(user);
        }

        public async Task<UserModel> AddUser(UserModel user)
        {
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
    }
}
