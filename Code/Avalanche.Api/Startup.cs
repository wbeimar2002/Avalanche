using AutoMapper;
using Avalanche.Api.Extensions;
using Avalanche.Api.Hubs;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Managers.Notifications;
using Avalanche.Api.Managers.Settings;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Settings;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Broadcaster.Services;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using static AvidisDeviceInterface.V1.Protos.Avidis;
using static Ism.PatientInfoEngine.V1.Protos.PatientListService;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Recorder.Core.V1.Protos.Recorder;
using static Ism.Routing.V1.Protos.Routing;
using static Ism.Storage.Core.Configuration.V1.Protos.ConfigurationService;
using static Ism.Storage.Core.PatientList.V1.Protos.PatientListStorage;
using static Ism.Streaming.V1.Protos.WebRtcStreamer;

namespace Avalanche.Api
{
    [ExcludeFromCodeCoverage]
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

            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = 209715200;
            });

            IConfigurationService configurationService = new ConfigurationService(Configuration);
            services.AddSingleton(c => configurationService);

            var grpcCertificate = configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = configurationService.GetEnvironmentVariable("grpcPassword");
            var grpcServerValidationCertificate = configurationService.GetEnvironmentVariable("grpcServerValidationCertificate");

            services.AddSingleton<ISettingsManager, SettingsManagerMock>();
            services.AddSingleton<IPatientsManager, PatientsManager>();
            services.AddSingleton<IPhysiciansManager, PhysiciansManager>();
            services.AddSingleton<IProceduresManager, ProceduresManagerMock>();
            services.AddSingleton<IMetadataManager, MetadataManager>();
            services.AddSingleton<ILicensingManager, LicensingManagerMock>();
            services.AddSingleton<IMediaService, MediaService>();
            services.AddSingleton<IDevicesManager, DevicesManager>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<IPieService, PieService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IBroadcastService, BroadcastService>();
            services.AddSingleton<INotificationsManager, NotificationsManager>();
            services.AddSingleton<IRoutingService, RoutingService>();
            services.AddSingleton<IAvidisService, AvidisService>();
            services.AddSingleton<IRecorderService, RecorderService>();
            services.AddSingleton<ICertificateProvider>(new FileSystemCertificateProvider(grpcCertificate, grpcPassword, grpcServerValidationCertificate));
            services.AddSingleton<IGrpcClientFactory<PatientListServiceClient>, GrpcClientFactory<PatientListServiceClient>>();
            services.AddSingleton<IGrpcClientFactory<PatientListStorageClient>, GrpcClientFactory<PatientListStorageClient>>();
            services.AddSingleton<IGrpcClientFactory<RecorderClient>, GrpcClientFactory<RecorderClient>>();
            services.AddSingleton<IGrpcClientFactory<RoutingClient>, GrpcClientFactory<RoutingClient>>();
            services.AddSingleton<IGrpcClientFactory<AvidisClient>, GrpcClientFactory<AvidisClient>>();
            services.AddSingleton<IGrpcClientFactory<WebRtcStreamerClient>, GrpcClientFactory<WebRtcStreamerClient>>();
            services.AddSingleton<IGrpcClientFactory<PgsTimeoutClient>, GrpcClientFactory<PgsTimeoutClient>>();
            services.AddSingleton<IGrpcClientFactory<ConfigurationServiceClient>, GrpcClientFactory<ConfigurationServiceClient>>();

            services.AddHttpContextAccessor();
            services.AddSingleton<IAccessInfoFactory, AccessInfoFactory>();

            //TODO: Check this. Should be env variables?
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

            services.AddAutoMapper(typeof(Startup));

            ConfigureAuthorization(services);
            ConfigureCorsPolicy(services, configurationService);
            ConfigureCertificate(configurationService);
        }

        private void ConfigureCertificate(IConfigurationService configurationService)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            var grpcCertificate = configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = configurationService.GetEnvironmentVariable("grpcPassword");
            var grpcThumprint = configurationService.GetEnvironmentVariable("grpcThumprint");

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, grpcThumprint, false);
            if (certificates.Count <= 0)
            {
                store.Add(new X509Certificate2(grpcCertificate, grpcPassword, X509KeyStorageFlags.PersistKeySet));
            }
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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsApiPolicy",
                builder =>
                {
                    builder
                        //.WithOrigins("http://localhost:4200")
                        //.WithHeaders(new[] { "authorization", "content-type", "accept" })
                        //.WithMethods(new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" })
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                        //.AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            } // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts


            app.UseSerilogRequestLogging();

            app.UseCustomSwagger();
            
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("CorsApiPolicy");
            app.UseFileServer();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BroadcastHub>("/broadcast");
                endpoints.MapControllers();
            });
        }
    }
}
