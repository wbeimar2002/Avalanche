using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Api.Broadcaster.Services;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Managers.Security;
using Avalanche.Api.Managers.Settings;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Avalanche.Api
{
    public class Startup
    {
        static readonly string __secretKey = "SigningKeyThatIsFromTheEnvironmentAvalanche"; //TODO: Check this
        static readonly SymmetricSecurityKey __signingKey = new SymmetricSecurityKey(key: Encoding.ASCII.GetBytes(__secretKey));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtAppSettings = Configuration.GetSection(nameof(JwtIssuerOptions));

            services.AddControllers();
            services.AddSignalR();

            //Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Avalanche.Api", Version = "V1" });
            });

            IConfigurationService configurationService = new ConfigurationService(Configuration);
            services.AddSingleton<IConfigurationService>(c => configurationService);

            services.AddSingleton<ISettingsManager, SettingsManagerMock>();
            services.AddSingleton<ISecurityManager, SecurityManagerMock>();
            services.AddSingleton<IPatientsManager, PatientsManagerMock>();
            services.AddSingleton<IPhysiciansManager, PhysiciansManagerMock>();
            services.AddSingleton<IProceduresManager, ProceduresManagerMock>();
            services.AddSingleton<IMetadataManager, MetadataManagerMock>();
            services.AddSingleton<ILicensingManager, LicensingManagerMock>();

            services.AddSingleton<IAuthorizationServiceClient, AuthorizationServiceClient>();
            services.AddSingleton<IBroadcastService, BroadcastService>();

            ConfigureCorsPolicy(services, configurationService);

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = GetTokenValidationParams(jwtAppSettings);
                });

            services.AddAuthorization(opt => { opt.AddPolicy(ConstantsHelper.ADMIN_POLICY_NAME, policy => policy.RequireClaim("UserType", "AvalancheAdmin")); });

            services.Configure<JwtIssuerOptions>(o =>
            {
                o.Issuer = jwtAppSettings[nameof(JwtIssuerOptions.Issuer)];
                o.Audience = jwtAppSettings[nameof(JwtIssuerOptions.Audience)];
                o.SigningCredentials = new SigningCredentials(__signingKey, SecurityAlgorithms.HmacSha256);
            });
        }

        private static void ConfigureCorsPolicy(IServiceCollection services, IConfigurationService configurationService)
        {
            /*
             * This was implemented in Hikari because people use the API from an external computer,
             * please evaluate if this scenario applies here in Avalanche
             
            var configSettings = await configurationService.LoadAsync<ConfigSettings>("/environment/env.json");

            if (configSettings == null)
                configSettings = new ConfigSettings();

            //TODO: This needs to be reviewed ports should not be hardcoded
            configSettings.IpAddress = configSettings.IpAddress.Replace(":6005", ":8443");*/

            // Add Cors
            // https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        "https://localhost:8443",
                        "http://localhost:4200") //Dev Mode
                        //configSettings.IpAddress)
                    //.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Avalanche.Api V1");
                c.RoutePrefix = string.Empty;
            });
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static TokenValidationParameters GetTokenValidationParams(IConfiguration jwtAppSettings) => new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtAppSettings[nameof(JwtIssuerOptions.Issuer)],

            ValidateAudience = true,
            ValidAudience = jwtAppSettings[nameof(JwtIssuerOptions.Audience)],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = __signingKey,

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    }
}
