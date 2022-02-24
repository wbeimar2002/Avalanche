using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Handlers.Security.Tokens;
using Avalanche.Api.Services.Security;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.Managers
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IMapper _mapper;
        private readonly ITokenHandler _tokenHandler;
        private readonly ISecurityService _usersService;
        public AuthenticationManager(ISecurityService usersService, ITokenHandler tokenHandler, IMapper mapper)
        {
            _tokenHandler = tokenHandler;
            _usersService = usersService;
            _mapper = mapper;
        }

        public async Task<TokenResponseViewModel> CreateAccessTokenAsync(string userName, string password)
        {
            var result = await _usersService.VerifyUserLogin(userName, password).ConfigureAwait(false);
            if (!result.LoginValid)
            {
                return new TokenResponseViewModel(false, "Invalid credentials.", null);
            }

            var token = _tokenHandler.CreateAccessToken(_mapper.Map<UserModel>(result.User));

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

            var response = await _usersService.GetUser(userName).ConfigureAwait(false);
            if (response.User == null)
            {
                return new TokenResponseViewModel(false, "Invalid refresh token.", null);
            }

            var accessToken = _tokenHandler.CreateAccessToken(_mapper.Map<UserModel>(response.User));
            return new TokenResponseViewModel(true, null, accessToken);
        }

        public void RevokeRefreshToken(string refreshToken) =>
            _tokenHandler.RevokeRefreshToken(refreshToken);
    }
}
