using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.ViewModels;

namespace Avalanche.Security.Server.Managers
{
    //TODO: Review this, is a little bit different to the API controllers/managers but chaange this in this moment can affect the demo
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHandler _tokenHandler;

        public AuthenticationManager(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenHandler tokenHandler)
        {
            _tokenHandler = tokenHandler;
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
        }

        public async Task<TokenResponseViewModel> CreateAccessTokenAsync(string userName, string password)
        {
            var user = await _userRepository.FindByUserNameAsync(userName);

            if (user == null || !_passwordHasher.PasswordMatches(password, user.Password))
            {
                return new TokenResponseViewModel(false, "Invalid credentials.", null);
            }

            var token = _tokenHandler.CreateAccessToken(user);

            return new TokenResponseViewModel(true, null, token);
        }

        public async Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken, string userEmail)
        {
            var token = _tokenHandler.TakeRefreshToken(refreshToken);

            if (token == null)
            {
                return new TokenResponseViewModel(false, "Invalid refresh token.", null);
            }

            if (token.IsExpired())
            {
                return new TokenResponseViewModel(false, "Expired refresh token.", null);
            }

            var user = await _userRepository.FindByUserNameAsync(userEmail);
            if (user == null)
            {
                return new TokenResponseViewModel(false, "Invalid refresh token.", null);
            }

            var accessToken = _tokenHandler.CreateAccessToken(user);
            return new TokenResponseViewModel(true, null, accessToken);
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _tokenHandler.RevokeRefreshToken(refreshToken);
        }
    }
}
