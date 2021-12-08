using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.Core.Security.Tokens
{
    public interface ITokenHandler
    {
        AccessToken CreateAccessToken(UserModel user);
        RefreshToken TakeRefreshToken(string token);
        void RevokeRefreshToken(string token);
    }
}
