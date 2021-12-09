
using Avalanche.Api.ViewModels.Security;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.Handlers.Security.Tokens
{
    public interface ITokenHandler
    {
        AccessToken CreateAccessToken(UserModel user);
        RefreshToken TakeRefreshToken(string token);
        void RevokeRefreshToken(string token);
    }
}
