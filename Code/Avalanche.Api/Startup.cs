using AutoMapper;
using Avalanche.Api.Extensions;
using Avalanche.Api.Hubs;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Managers.Notifications;
using Avalanche.Api.Managers.PgsTimeout;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Notifications;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Broadcaster.Services;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Interfaces;
using Ism.SystemState.Client;
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
using System.Threading.Tasks;
using static AvidisDeviceInterface.V1.Protos.Avidis;
using static Ism.PatientInfoEngine.V1.Protos.PatientListService;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Recorder.Core.V1.Protos.Recorder;
using static Ism.Routing.V1.Protos.Routing;
using static Ism.Storage.Core.Configuration.V1.Protos.ConfigurationService;
using static Ism.Storage.Core.DataManagement.V1.Protos.DataManagementStorage;
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

            services.AddSingleton<IMaintenanceManager, MaintenanceManager>();
            services.AddSingleton<IPatientsManager, PatientsManager>();
            services.AddSingleton<IPhysiciansManager, PhysiciansManager>();
            services.AddSingleton<IMetadataManager, MetadataManager>();
            services.AddSingleton<ILicensingManager, LicensingManagerMock>();
            services.AddSingleton<IMediaService, MediaService>();
            services.AddSingleton<IDevicesManager, DevicesManager>();
            services.AddSingleton<IPgsTimeoutManager, PgsTimeoutManager>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<IProceduresManager, ProceduresManager>();
            services.AddSingleton<IPieService, PieService>();
            services.AddSingleton<IBroadcastService, BroadcastService>();
            services.AddSingleton<INotificationsManager, NotificationsManager>();
            services.AddSingleton<IRoutingService, RoutingService>();
            services.AddSingleton<IAvidisService, AvidisService>();
            services.AddSingleton<IRecorderService, RecorderService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IPgsTimeoutService, PgsTimeoutService>();
            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<ICertificateProvider>(new FileSystemCertificateProvider(grpcCertificate, grpcPassword, grpcServerValidationCertificate));
            services.AddSingleton<IGrpcClientFactory<DataManagementStorageClient>, GrpcClientFactory<DataManagementStorageClient>>();
            services.AddSingleton<IGrpcClientFactory<PatientListServiceClient>, GrpcClientFactory<PatientListServiceClient>>();
            services.AddSingleton<IGrpcClientFactory<PatientListStorageClient>, GrpcClientFactory<PatientListStorageClient>>();
            services.AddSingleton<IGrpcClientFactory<RecorderClient>, GrpcClientFactory<RecorderClient>>();
            services.AddSingleton<IGrpcClientFactory<RoutingClient>, GrpcClientFactory<RoutingClient>>();
            services.AddSingleton<IGrpcClientFactory<AvidisClient>, GrpcClientFactory<AvidisClient>>();
            services.AddSingleton<IGrpcClientFactory<WebRtcStreamerClient>, GrpcClientFactory<WebRtcStreamerClient>>();
            services.AddSingleton<IGrpcClientFactory<PgsTimeoutClient>, GrpcClientFactory<PgsTimeoutClient>>();
            services.AddSingleton<IGrpcClientFactory<ConfigurationServiceClient>, GrpcClientFactory<ConfigurationServiceClient>>();
            services.AddSingleton<IAccessInfoFactory, AccessInfoFactory>();

            services.AddHostedService<NotificationsListener>();

            services.AddAutoMapper(typeof(Startup));

            var stateServiceAddress = configurationService.GetEnvironmentVariable("stateServiceGrpcAddress");
            var stateServicePort = configurationService.GetEnvironmentVariable("stateServiceGrpcPort");
            services.AddGrpcStateClient(stateServiceAddress, uint.Parse(stateServicePort), "AvalancheApi");

            ConfigureAuthorization(services);
            ConfigureCorsPolicy(services);
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

                // From: https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1
                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                jwtBearerOptions.Events = new JwtBearerEvents
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
                        .WithOrigins("https://localhost:4200")
                        //.WithHeaders(new[] { "authorization", "content-type", "accept" })
                        //.WithMethods(new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" })
                        .AllowAnyHeader()
                        //.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowCredentials();
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
            app.UseCors("CorsApiPolicy"); // NOTE: cors must come before Authorization in the request pipeline

            app.UseAuthorization();

            app.UseFileServer();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BroadcastHub>(BroadcastHub.BroadcastHubRoute);
                endpoints.MapControllers();
            });
        }
    }
}
