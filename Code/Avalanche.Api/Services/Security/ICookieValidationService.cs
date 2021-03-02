using System.Security.Claims;

namespace Avalanche.Api.Services.Security
{
    public interface ICookieValidationService
    {
        void RevokePrincipal(ClaimsPrincipal claimsPrincipal);
        bool ValidatePrincipal(ClaimsPrincipal claimsPrincipal);
    }
}