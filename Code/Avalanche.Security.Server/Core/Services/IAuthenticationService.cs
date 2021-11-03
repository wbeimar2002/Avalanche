using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Services.Communication;

namespace Avalanche.Security.Server.Core.Services
{
    public interface IAuthenticationService
    {
         Task<TokenResponse> CreateAccessTokenAsync(string loginName, string password);
         Task<TokenResponse> RefreshTokenAsync(string refreshToken, string loginName);
         void RevokeRefreshToken(string refreshToken);
    }
}
