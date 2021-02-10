using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Services.Communication;

namespace Avalanche.Security.Server.Core.Services
{
    public interface IAuthenticationService
    {
        Task<TokenResponse> CreateAccessTokenAsync(string email, string password);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken, string userEmail);
        Task RevokeRefreshToken(string refreshToken);
    }
}