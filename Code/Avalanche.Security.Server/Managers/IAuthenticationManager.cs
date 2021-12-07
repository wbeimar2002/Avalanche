using System.Threading.Tasks;
using Avalanche.Security.Server.ViewModels;

namespace Avalanche.Security.Server.Core.Services
{
    public interface IAuthenticationManager
    {
        Task<TokenResponseViewModel> CreateAccessTokenAsync(string userName, string password);
        Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken, string userEmail);
        void RevokeRefreshToken(string refreshToken);
    }
}
