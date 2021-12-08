using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Extensions;
using Avalanche.Security.Server.Options;
using Avalanche.Security.Server.Security.Hashing;
using Avalanche.Security.Server.Managers;
using Avalanche.Security.Server.V1.Handlers;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Options;

using Ism.Common.Core.Configuration.Extensions;
using Ism.Common.Core.Extensions;
using Ism.Storage.Core.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Ism.Storage.Core.Infrastructure.Interfaces;

namespace Avalanche.Security.Server
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string SecurityManagementDatabaseName = "security.db";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Libraries
            _ = services.AddGrpc();

            _ = services.AddAutoMapper(GetType().Assembly);
            _ = services.AddCustomSwagger();

            // Scoped
            _ = services.AddScoped<IUserRepository, UserRepository>();
            _ = services.AddScoped<IAuthenticationManager, AuthenticationManager>();

            // Singleton
            _ = services.AddSingleton<IDatabaseWriter<SecurityManagementContext>, DatabaseWriter<SecurityManagementContext>>();
            _ = services.AddSingleton<IPasswordHasher, PasswordHasher>();
            _ = services.AddSingleton<ITokenHandler, Security.Tokens.TokenHandler>();

            // Configuration
            _ = services.AddConfigurationPoco<TokenAuthConfiguration>(_configuration, nameof(TokenAuthConfiguration));
            _ = services.AddConfigurationPoco<AuthConfiguration>(_configuration, nameof(AuthConfiguration));
            _ = services.AddSingleton(sp => new SigningOptions(sp.GetRequiredService<AuthConfiguration>().SecretKey));
            _ = services.AddConfigurationLoggingOnStartup();

            // ASP.NET Features
            _ = services.AddControllers();
            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

            _ = services.ConfigureOptions<ConfigureJwtBearerOptions>();

            ConfigureCorsPolicy(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var dbManager = new DatabaseMigrationManager(
                app.ApplicationServices.GetRequiredService<ILogger<DatabaseMigrationManager>>()
            );

            _ = dbManager.UpgradeDatabase(GetSecurityManagementDatabaseLocation(), typeof(SecurityManagementContext).Assembly);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            } // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts

            app.UseCors("CorsApiPolicy"); // NOTE: cors must come before Authorization in the request pipeline
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseCustomSwagger();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                _ = endpoints.MapGrpcService<UsersManagementServiceHandler>();
            });
        }

        private static void ConfigureCorsPolicy(IServiceCollection services)
        {
            // Add Cors
            // https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
            services.AddCors(options =>
            {
                options.AddPolicy("CorsApiPolicy",
                builder =>
                {
                    // TODO: this still is not correct for remote clients...not sure how to handle that if web is being served from separate endpoint to api, since we do not have a well-known address.
                    builder
                        .WithOrigins("https://localhost:4200", "http://localhost:4200", "http://localhost:8080", "http://localhost:8082")
                        .AllowAnyHeader()
                        //.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        private string GetDatabaseLocation(string database) => Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location) ?? _environment.ContentRootPath, "database", database);

        private string GetSecurityManagementDatabaseLocation() => GetDatabaseLocation(SecurityManagementDatabaseName);
    }
}
