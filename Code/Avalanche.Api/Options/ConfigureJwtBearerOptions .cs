using Avalanche.Api.Hubs;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

using System.Threading.Tasks;

namespace Avalanche.Api.Options
{
    public class ConfigureJwtBearerOptions: IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly SigningOptions _signingOptions;
        private readonly TokenAuthConfiguration _tokenConfiguration;
        public ConfigureJwtBearerOptions(TokenAuthConfiguration tokenConfiguration, SigningOptions signingOptions)
{
            _tokenConfiguration = tokenConfiguration;
            _signingOptions = signingOptions;
}
        public void Configure(string name, JwtBearerOptions options)
        {
            options.TokenValidationParameters = JwtUtilities.GetDefaultJwtValidationParameters(_tokenConfiguration, _signingOptions);

            // From: https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1
            // We have to hook the OnMessageReceived event in order to
            // allow the JWT authentication handler to read the access
            // token from the query string when a WebSocket or 
            // Server-Sent Events request comes in.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        if (path.StartsWithSegments(BroadcastHub.BroadcastHubRoute))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(JwtBearerDefaults.AuthenticationScheme, options);
        }
    }
}
