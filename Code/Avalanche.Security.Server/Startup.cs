using Avalanche.Security.Server.Core.Repositories;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Extensions;
using Avalanche.Security.Server.Options;
using Avalanche.Security.Server.Persistence;
using Avalanche.Security.Server.Security.Hashing;
using Avalanche.Security.Server.Services;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Options;

using Ism.Common.Core.Configuration.Extensions;
using Ism.Common.Core.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Avalanche.Security.Server
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private const string SecurityDatabaseName = "security.db";

        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Libraries
            services.AddDbContext<SecurityDbContext>(options =>
                  options.UseSqlite(MakeConnectionString(GetDatabaseLocation(SecurityDatabaseName))));
            services.AddAutoMapper(GetType().Assembly);
            services.AddCustomSwagger();

            // Scoped
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Singleton
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenHandler, Security.Tokens.TokenHandler>();

            // Configuration
            services.AddConfigurationPoco<TokenAuthConfiguration>(_configuration, nameof(TokenAuthConfiguration));
            services.AddConfigurationPoco<AuthConfiguration>(_configuration, nameof(AuthConfiguration));
            services.AddSingleton(sp => new SigningOptions(sp.GetRequiredService<AuthConfiguration>().SecretKey));
            services.AddConfigurationLoggingOnStartup();

            // ASP.NET Features
            services.AddControllers();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();
            services.ConfigureOptions<ConfigureJwtBearerOptions>();

            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder
                //.WithOrigins("https://localhost:4200") //Dev Mode
                //configSettings.IpAddress)
                //.AllowAnyOrigin()
                .SetIsOriginAllowed(s => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var dbManager = new DatabaseMigrationManager();
            _ = dbManager.UpgradeDatabase(GetDatabaseLocation(SecurityDatabaseName), typeof(SecurityDbContext).Assembly);

            var context = app.ApplicationServices.GetService<SecurityDbContext>();
            var passwordHasher = app.ApplicationServices.GetService<IPasswordHasher>();
            DatabaseSeed.Seed(context, passwordHasher);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            } // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts

            app.UseCors();
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseCustomSwagger();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static string MakeConnectionString(string databasePath) => $"Data Source={databasePath}";

        private string GetDatabaseLocation(string database, string subDirectory = "database") => Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location) ?? _environment.ContentRootPath, subDirectory, database);
    }
}
