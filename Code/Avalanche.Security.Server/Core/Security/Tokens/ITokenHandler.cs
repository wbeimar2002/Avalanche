using Avalanche.Security.Server.Entities;

namespace Avalanche.Security.Server.Core.Security.Tokens
{
    public interface ITokenHandler
    {
         AccessToken CreateAccessToken(UserEntity user);
         RefreshToken TakeRefreshToken(string token);
         void RevokeRefreshToken(string token);
    }
}
