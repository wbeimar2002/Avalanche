using Avalanche.Shared.Infrastructure.Models;
using Ism.Utility.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Utilities
{
    public class JwtUtilities
    {
        public static TokenValidationParameters GetDefaultJwtValidationParameters(TokenOptions tokenOptions, SigningConfigurations signingConfigurations)
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
                ClockSkew = TimeSpan.Zero
            };
        }

        public static ClaimsPrincipal ValidateToken(string jwtToken, TokenValidationParameters tokenValidationParameters)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(jwtToken, tokenValidationParameters, out _);
        }

    }
}
