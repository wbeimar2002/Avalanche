using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;

namespace Avalanche.Shared.Infrastructure.Options
{
    public class ConfigureJwtBearerOptions: IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly SigningOptions _signingOptions;
        private readonly TokenConfiguration _tokenConfiguration;
        public ConfigureJwtBearerOptions(TokenConfiguration tokenConfiguration, SigningOptions signingOptions)
{
            _tokenConfiguration = tokenConfiguration;
            _signingOptions = signingOptions;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _tokenConfiguration.Issuer,
                    ValidAudience = _tokenConfiguration.Audience,
                    IssuerSigningKey = _signingOptions.Key,
                    ClockSkew = TimeSpan.Zero
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(JwtBearerDefaults.AuthenticationScheme, options);
        }
    }
}
