using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Repositories;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Core.Services.Communication;

namespace Avalanche.Security.Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<CreateUserResponse> CreateUserAsync(User user, params ERole[] userRoles)
        {
            var existingUser = await _userRepository.FindByLoginAsync(user.LoginName).ConfigureAwait(false);
            if(existingUser != null)
            {
                return new CreateUserResponse(false, "Login name already in use.", null);
            }

            user.Password = _passwordHasher.HashPassword(user.Password);

            await _userRepository.AddAsync(user, userRoles).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return new CreateUserResponse(true, null, user);
        }

        public async Task<User> FindByLoginAsync(string loginName) => await _userRepository.FindByLoginAsync(loginName).ConfigureAwait(false);

        public async Task<List<User>> GetUsers(UserFilterModel filter) => await _userRepository.GetUsers(filter).ConfigureAwait(false);
    }
}
