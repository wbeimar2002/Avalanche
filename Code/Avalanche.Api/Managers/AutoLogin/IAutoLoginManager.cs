using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers.AutoLogin
{
    public interface IAutoLoginManager
    {
        Task<bool> AutoLoginEnabled();
        Task<TokenResponseViewModel> CreateAccessTokenAsync();
        Task<TokenResponseViewModel> RefreshTokenAsync(string refreshToken);
        void RevokeRefreshToken(string refreshToken);
    }
}
