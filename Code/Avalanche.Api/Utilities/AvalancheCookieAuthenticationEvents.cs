using Avalanche.Api.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace Avalanche.Api.Utilities
{
    public class AvalancheCookieAuthenticationEvents: CookieAuthenticationEvents
    {
        private ICookieValidationService _cookieValidationService;

        public AvalancheCookieAuthenticationEvents(ICookieValidationService cookieValidationService)
        {
            _cookieValidationService = cookieValidationService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (!_cookieValidationService.ValidatePrincipal(context.Principal))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
