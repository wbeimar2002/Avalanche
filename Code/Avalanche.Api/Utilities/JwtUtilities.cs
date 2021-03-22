using Avalanche.Shared.Infrastructure.Options;

using Ism.Utility.Core;

using Microsoft.IdentityModel.Tokens;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Avalanche.Api.Utilities
{
    public class JwtUtilities
    {
        public static TokenValidationParameters GetDefaultJwtValidationParameters(TokenConfiguration tokenOptions, SigningOptions signingConfigurations)
        {
            Preconditions.ThrowIfNull(nameof(tokenOptions), tokenOptions);
            Preconditions.ThrowIfNull(nameof(signingConfigurations), signingConfigurations);

            return new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = tokenOptions.Issuer,
                ValidAudience = tokenOptions.Audience,
                IssuerSigningKey = signingConfigurations.Key,
                ClockSkew = TimeSpan.Zero,
            };
        }

        public static ClaimsPrincipal ValidateToken(string jwtToken, TokenValidationParameters tokenValidationParameters)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear(); // this prevents mangling of some claim types like "sub"

            return handler.ValidateToken(jwtToken, tokenValidationParameters, out _);
        }

    }
}
 