using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ism.Broadcaster.Services;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Managers.Settings;
using Avalanche.Api.Services.Configuration;
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
using Microsoft.AspNetCore.Authorization;
using Avalanche.Api.Handlers;
using Avalanche.Api.Extensions;
using Avalanche.Api.Hubs;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using Avalanche.Api.Managers.Devices;

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

            services.AddCustomSwagger();
            services.AddHttpContextAccessor();

            IConfigurationService configurationService = new ConfigurationService(Configuration);
            services.AddSingleton(c => configurationService);

            services.AddSingleton<ISettingsManager, SettingsManagerMock>();
            services.AddSingleton<IPatientsManager, PatientsManagerMock>();
            services.AddSingleton<IPhysiciansManager, PhysiciansManagerMock>();
            services.AddSingleton<IProceduresManager, ProceduresManagerMock>();
            services.AddSingleton<IMetadataManager, MetadataManagerMock>();
            services.AddSingleton<ILicensingManager, LicensingManagerMock>();
            services.AddSingleton<IOutputsManager, OutputsManagerMock>();

            services.AddSingleton<IBroadcastService, BroadcastService>();

            var hostName = configurationService.GetValue<string>("RabbitMqOptions:HostName");
            var port = configurationService.GetValue<int>("RabbitMqOptions:Port");
            var managementPort = configurationService.GetValue<int>("RabbitMqOptions:ManagementPort");
            var userName = configurationService.GetValue<string>("RabbitMqOptions:UserName");
            var password = configurationService.GetValue<string>("RabbitMqOptions:Password");
            var queueName = configurationService.GetValue<string>("RabbitMqOptions:QueueName");

            services.Configure<RabbitMqOptions>(options =>
            {
                options.HostName = hostName;
                options.ManagementPort = managementPort;
                options.UserName = userName;
                options.Password = password;
                options.QueueName = queueName;
                options.Port = port;
            });

            services.AddSingleton<IRabbitMqClientService, RabbitMqClientService>();

            ConfigureAuthorization(services);
            ConfigureCorsPolicy(services, configurationService);
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            services.Configure<TokenOptions>(Configuration.GetSection("TokenOptions"));
            var tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

            services.Configure<TokenOptions>(Configuration.GetSection("AuthSettings"));
            var authSettings = Configuration.GetSection("AuthSettings").Get<AuthSettings>();

            var signingConfigurations = new SigningConfigurations(authSettings.SecretKey);
            services.AddSingleton(signingConfigurations);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience,
                    IssuerSigningKey = signingConfigurations.Key,
                    ClockSkew = TimeSpan.Zero
                };
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
                //.WithOrigins("https://localhost:4200") //Dev Mode
                //configSettings.IpAddress)
                //.AllowAnyOrigin()
                .SetIsOriginAllowed(s => true)
                    .WithMethods("POST", "GET", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .AllowAnyHeader()
                    .AllowCredentials()
                    ;
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseCustomSwagger();
            
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseFileServer();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BroadcastHub>("/broadcast");
                endpoints.MapControllers();
            });
        }
    }
}
