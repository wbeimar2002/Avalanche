using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Constants;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Security
{
    public class SecurityManager : ISecurityManager
    {
        readonly ICookieValidationService _cookieValidationService;
        readonly SigningConfigurations _signingConfigurations;
        readonly IOptions<TokenOptions> _tokenOptions;
        readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityManager(SigningConfigurations signingConfigurations,
            IHttpContextAccessor httpContextAccessor,
            IOptions<TokenOptions> tokenOptions)
        {
            _signingConfigurations = signingConfigurations;
            _httpContextAccessor = httpContextAccessor;
            _tokenOptions = tokenOptions;
        }

        public ClaimsIdentity CreateTokenIdentity(string jwtToken, string authenticationScheme)
        {
            var tokenUser = JwtUtilities.ValidateToken(jwtToken, JwtUtilities.GetDefaultJwtValidationParameters(_tokenOptions.Value, _signingConfigurations));

            return new ClaimsIdentity(tokenUser.Claims, authenticationScheme);
        }

        public ClaimsIdentity AcquireFileCookie(string jwtToken)
        {
            var identity = CreateTokenIdentity(jwtToken, CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(AvalancheClaimTypes.LastChanged, DateTimeOffset.Now.ToString()));
            return identity;
        }

        public void RevokeFileCookie()
        {
            var user = _httpContextAccessor.HttpContext.User;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                _cookieValidationService.RevokePrincipal(user);
            }
        }
    }
}
