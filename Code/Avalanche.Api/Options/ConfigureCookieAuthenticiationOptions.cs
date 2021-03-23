using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Models;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using System;

namespace Avalanche.Api.Options
{
    public class ConfigureCookieAuthenticiationOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly CookieAuthConfiguration _cookieConfig;
        public ConfigureCookieAuthenticiationOptions(CookieAuthConfiguration cookieConfig)
{
            _cookieConfig = cookieConfig;
}
        public void Configure(string name, CookieAuthenticationOptions options)
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromSeconds(_cookieConfig.ExpirationSeconds);

            options.Cookie.Path = "/api" + _cookieConfig.Path;
            options.LoginPath = "/login"; // this is route to angular app login page

            // forward anything not to the cookie path to the jwt auth handler
            options.ForwardDefaultSelector = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments(_cookieConfig.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                return JwtBearerDefaults.AuthenticationScheme;
            };

            options.EventsType = typeof(AvalancheCookieAuthenticationEvents);
        }

        public void Configure(CookieAuthenticationOptions options)
        {
            Configure(CookieAuthenticationDefaults.AuthenticationScheme, options);
        }
    }
}
