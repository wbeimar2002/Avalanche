using System.Security.Claims;

namespace Avalanche.Api.Managers.Security
{
    public interface ISecurityManager
    {
        ClaimsIdentity CreateTokenIdentity(string jwtToken, string authenticationScheme);
        ClaimsIdentity AcquireFileCookie(string jwtToken);
        bool RevokeFileCookie();
    }
}