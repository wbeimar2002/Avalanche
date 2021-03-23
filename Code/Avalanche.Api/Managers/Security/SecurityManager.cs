using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Options;

using System.Security.Claims;

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
        }

        public ClaimsIdentity CreateTokenIdentity(string jwtToken, string authenticationScheme)
        {
            var tokenUser = JwtUtilities.ValidateToken(jwtToken, JwtUtilities.GetDefaultJwtValidationParameters(_tokenConfiguration, _signingConfigurations));

            return new ClaimsIdentity(tokenUser.Claims, authenticationScheme);
        }
    }
}
