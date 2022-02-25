using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalanche.Security.Server.Core;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Managers;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Validators;
using Avalanche.Security.Server.V1.Handlers;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Options;
using Avalanche.Shared.Infrastructure.Security.Hashing;
using FluentValidation;
using Ism.Common.Core.Configuration.Extensions;
using Ism.Common.Core.Extensions;
using Ism.Storage.Core.Infrastructure;
using Ism.Storage.Core.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Avalanche.Security.Server
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string SecurityDatabaseName = "security.db";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddGrpc();
            _ = services.AddControllers();
            _ = services.AddAutoMapper(GetType().Assembly);
            _ = services.AddAutoMapper(typeof(Core.Mappings.UserMapProfile).Assembly);
            _ = services.AddDbContext<SecurityDbContext>(options => options.UseSqlite(DatabaseMigrationManager.MakeConnectionString(GetSecurityDatabaseLocation())));

            // Singleton
            _ = services.AddSingleton<IDatabaseWriter<SecurityDbContext>, DatabaseWriter<SecurityDbContext>>();

            // Configuration
            _ = services.AddConfigurationLoggingOnStartup();

            // Transient
            _ = services.AddTransient<IPasswordHasher, BcryptPasswordHasher>();
            _ = services.AddTransient<IUsersManager, UsersManager>();
            _ = services.AddTransient<IUserRepository, UserRepository>();

            // Validation
            _ = services.AddTransient<IValidator<NewUserModel>, NewUserValidator>();
            _ = services.AddTransient<IValidator<UpdateUserModel>, UpdateUserValidator>();
            _ = services.AddTransient<IValidator<UpdateUserPasswordModel>, UpdateUserPasswordValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var dbManager = new DatabaseMigrationManager(
                app.ApplicationServices.GetRequiredService<ILogger<DatabaseMigrationManager>>()
            );

            _ = dbManager.UpgradeDatabase(GetSecurityDatabaseLocation(), typeof(SecurityDbContext).Assembly);

            var context = app.ApplicationServices.GetRequiredService<SecurityDbContext>();
            var passwordHasher = app.ApplicationServices.GetRequiredService<IPasswordHasher>();

            DatabaseSeed.Seed(context, passwordHasher);

            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }

            _ = app.UseSerilogRequestLogging();
            _ = app.UseRouting();

            _ = app.UseEndpoints(endpoints =>
              {
                  _ = endpoints.MapControllers();

                  _ = endpoints.MapGrpcService<SecurityServiceHandler>();
              });
        }

        private string GetDatabaseLocation(string database) => Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location) ?? _environment.ContentRootPath, "database", database);

        private string GetSecurityDatabaseLocation() => GetDatabaseLocation(SecurityDatabaseName);
    }
}
