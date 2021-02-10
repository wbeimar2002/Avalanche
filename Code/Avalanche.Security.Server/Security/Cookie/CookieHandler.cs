using Avalanche.Security.Server.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Security.Server.Security.Cookie
{
    public class CookieHandler : ICookieHandler
    {
        private readonly IClaimMapper _claimMapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieHandler(IClaimMapper claimMapper, IHttpContextAccessor httpContextAccessor)
        {
            _claimMapper = claimMapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SignInUser(User user)
        {
            var claims = _claimMapper.GetClaims(user);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        public async Task SignOut()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
