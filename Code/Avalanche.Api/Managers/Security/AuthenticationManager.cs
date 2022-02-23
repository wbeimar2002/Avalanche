using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Handlers.Security.Tokens;
using Avalanche.Api.Services.Security;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Security.Hashing;

namespace Avalanche.Api.Managers
{
    //TODO: Review this, is a little bit different to the API controllers/managers but chaange this in this moment can affect the demo
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly ISecurityService _usersService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenHandler _tokenHandler;
        private readonly IMapper _mapper;

        public AuthenticationManager(ISecurityService usersService, IPasswordHasher passwordHasher, ITokenHandler tokenHandler, IMapper mapper)
        {
            _tokenHandler = tokenHandler;
            _passwordHasher = passwordHasher;
            _usersService = usersService;
            _mapper = mapper;
        }

        public async Task<TokenResponseViewModel> CreateAccessTokenAsync(string userName, string password)
        {
            var response = await _usersService.GetUser(userName);

            if (response.User == null || !_passwordHasher.PasswordMatches(password, response.User.Password))
            {
                return new TokenResponseViewModel(false, "Invalid credentials.", null);
            }

            var token = _tokenHandler.CreateAccessToken(_mapper.Map<UserModel>(response.User));

            return new TokenResponseViewModel(true, null, token);
        }

        public async Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken, string userName)
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

            var response = await _usersService.GetUser(userName);
            if (response.User == null)
            {
                return new TokenResponseViewModel(false, "Invalid refresh token.", null);
            }

            var accessToken = _tokenHandler.CreateAccessToken(_mapper.Map<UserModel>(response.User));
            return new TokenResponseViewModel(true, null, accessToken);
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _tokenHandler.RevokeRefreshToken(refreshToken);
        }
    }
}
