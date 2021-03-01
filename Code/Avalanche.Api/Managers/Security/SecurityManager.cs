using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Avalanche.Api.Managers.Security
{
    public class SecurityManager : ISecurityManager
    {
        private SigningConfigurations _signingConfigurations;
        private IOptions<TokenOptions> _tokenOptions;

        public SecurityManager(SigningConfigurations signingConfigurations, IOptions<TokenOptions> tokenOptions)
        {
            _signingConfigurations = signingConfigurations;
            _tokenOptions = tokenOptions;
        }

        public ClaimsIdentity CreateTokenIdentity(string jwtToken, string authenticationScheme)
        {
            var tokenUser = JwtUtilities.ValidateToken(jwtToken, JwtUtilities.GetDefaultJwtValidationParameters(_tokenOptions.Value, _signingConfigurations));

            return new ClaimsIdentity(tokenUser.Claims, authenticationScheme);
        }
    }
}
