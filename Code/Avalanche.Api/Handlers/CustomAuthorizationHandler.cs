using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Handlers
{
    public class CustomAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            /*var authFilterCtx = (Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)context.Resource;
            string authHeader = authFilterCtx.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.Contains("Bearer"))
            {
                var token = authHeader.Replace("Bearer", "");
                
            }

            throw new NotImplementedException();*/

            return Task.CompletedTask;
        }

        private JwtSecurityToken ValidateAndDecode(string jwt, IEnumerable<SecurityKey> signingKeys)
        {
            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = signingKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = "api://default",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = "https://{yourOktaDomain}/oauth2/default"
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
                // Or, you can return the ClaimsPrincipal
                // (which has the JWT properties automatically mapped to .NET claims)
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                throw new Exception($"Token was invalid: {argex.Message}");
            }
        }
    }
}
