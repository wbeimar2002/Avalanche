using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using Avalanche.Shared.Infrastructure.Options;

using System.Security.Claims;

namespace Avalanche.Api.Managers.Security
{
    public class SecurityManager : ISecurityManager
    {
        private readonly SigningOptions _signingConfigurations;
        private readonly TokenAuthConfiguration _tokenConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICookieValidationService _cookieValidationService;

        public SecurityManager(SigningOptions signingConfigurations,
            TokenAuthConfiguration tokenConfiguration,
            IHttpContextAccessor httpContextAccessor,
            ICookieValidationService cookieValidationService)
        {
            _signingConfigurations = signingConfigurations;
            _tokenConfiguration = tokenConfiguration;
            _httpContextAccessor = httpContextAccessor;
            _cookieValidationService = cookieValidationService;
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

        public bool RevokeFileCookie()
        {
            var user = _httpContextAccessor.HttpContext.User;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                _cookieValidationService.RevokePrincipal(user);
                return true;
            }

            return false;
        }
    }
}
