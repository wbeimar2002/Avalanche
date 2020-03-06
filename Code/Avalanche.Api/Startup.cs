using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ism.Api.Broadcaster.Services;
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
            services.AddSingleton(c => configurationService);

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
                    .WithOrigins("http://localhost:5001") //Dev Mode
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
