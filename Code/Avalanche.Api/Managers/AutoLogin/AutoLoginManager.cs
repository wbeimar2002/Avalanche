using System.Threading.Tasks;
using Avalanche.Api.Handlers.Security.Tokens;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.AutoLogin
{
    public class AutoLoginManager : IAutoLoginManager
    {
        private readonly ITokenHandler _tokenHandler;
        private readonly AutoLoginConfiguration _autoLoginConfiguration;

        public AutoLoginManager(ITokenHandler tokenHandler, AutoLoginConfiguration autoLoginConfiguration)
        {
            _tokenHandler = tokenHandler;
            _autoLoginConfiguration = autoLoginConfiguration;
        }

        public async Task<bool> AutoLoginEnabled() =>
            await Task.FromResult(_autoLoginConfiguration.AutoLogin).ConfigureAwait(false);

        public async Task<TokenResponseViewModel> CreateAccessTokenAsync()
        {
            var token = _tokenHandler.CreateAccessToken(new UserModel
            {
                AutoLogin = _autoLoginConfiguration.AutoLogin
            });

            return await Task.FromResult(new TokenResponseViewModel(true, null, token)).ConfigureAwait(false);
        }

        public async Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken)
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

            var accessToken = _tokenHandler.CreateAccessToken(new UserModel
            {
                AutoLogin = _autoLoginConfiguration.AutoLogin
            });

            return await Task.FromResult(new TokenResponseViewModel(true, null, accessToken)).ConfigureAwait(false);
        }

        public void RevokeRefreshToken(string refreshToken) =>
            _tokenHandler.RevokeRefreshToken(refreshToken);
    }
}
