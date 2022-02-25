using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers
{
    public interface IAuthenticationManager
    {
        Task<TokenResponseViewModel> CreateAccessTokenAsync(string userName, string password);
        Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken, string userName);
        void RevokeRefreshToken(string refreshToken);
    }
}
