using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Constants;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using Avalanche.Shared.Infrastructure.Options;

using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Security
{
    public class SecurityManager : ISecurityManager
    {
        private readonly SigningOptions _signingConfigurations;
        private readonly TokenAuthConfiguration _tokenConfiguration;

        public SecurityManager(SigningOptions signingConfigurations, TokenAuthConfiguration tokenConfiguration)
        {
            _signingConfigurations = signingConfigurations;
            _tokenConfiguration = tokenConfiguration;
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsIdentity CreateTokenIdentity(string jwtToken, string authenticationScheme)
        {
            var tokenUser = JwtUtilities.ValidateToken(jwtToken, JwtUtilities.GetDefaultJwtValidationParameters(_tokenConfiguration, _signingConfigurations));

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
