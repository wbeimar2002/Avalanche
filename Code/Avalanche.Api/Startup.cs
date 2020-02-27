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
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
            ConfigureAuthorization(services);
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            var jwtAppSettings = Configuration.GetSection(nameof(JwtIssuerOptions));
            
            services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.ASCII.GetBytes(jwtAppSettings[nameof(JwtIssuerOptions.SecurityKey)])),

                        // Clock skew compensates for server time drift.
                        // We recommend 5 minutes or less:
                        ClockSkew = TimeSpan.FromMinutes(5),
                        // Specify the key used to sign the token:
                        
                        RequireSignedTokens = true,
                        // Ensure the token hasn't expired:
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        // Ensure the token audience matches our audience value (default true):
                        ValidateAudience = true,
                        ValidAudience = jwtAppSettings[nameof(JwtIssuerOptions.Audience)],
                        // Ensure the token was issued by a trusted authorization server (default true):
                        ValidateIssuer = true,
                        ValidIssuer = jwtAppSettings[nameof(JwtIssuerOptions.Issuer)],
                    };
                });

            services.AddAuthorization(opt => { opt.AddPolicy(ConstantsHelper.ADMIN_POLICY_NAME, policy => policy.RequireClaim("UserType", "AvalancheAdmin")); });
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
    }
}
