using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Utility.Core;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Avalanche.Api.Services.Security
{
    public class CookieValidationService : ICookieValidationService
    {
        // TODO: Need persistant storage of this, once we have more user-management requirements defined. (But preserve the in-memory cache for performance.)
        private ConcurrentDictionary<string, DateTimeOffset> _userCookieLastModified = new ConcurrentDictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);

        public CookieValidationService() 
        { }

        public bool ValidatePrincipal(ClaimsPrincipal claimsPrincipal)
        {
            Preconditions.ThrowIfNull(nameof(claimsPrincipal), claimsPrincipal);

            var username = claimsPrincipal.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            else
            {
                if (_userCookieLastModified.TryGetValue(username, out var expiration))
                {
                    if (expiration < DateTimeOffset.Now)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RevokePrincipal(ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal?.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                var now = DateTimeOffset.Now;
                _userCookieLastModified.AddOrUpdate(username, now, (k, v) => now);
            }
        }
    }
}
