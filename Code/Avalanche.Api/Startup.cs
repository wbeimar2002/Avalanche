using Avalanche.Api.Extensions;
using Avalanche.Api.Hubs;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Notifications;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Notifications;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Broadcaster.Services;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Interfaces;
using Ism.SystemState.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
using System.Threading.Tasks;
using static AvidisDeviceInterface.V1.Protos.Avidis;
using static Ism.PatientInfoEngine.V1.Protos.PatientListService;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Recorder.Core.V1.Protos.Recorder;
using static Ism.Routing.V1.Protos.Routing;
using static Ism.Storage.Configuration.Client.V1.Protos.ConfigurationService;
using static Ism.Storage.DataManagement.Client.V1.Protos.DataManagementStorage;
using static Ism.Storage.PatientList.Client.V1.Protos.PatientListStorage;
using static Ism.Streaming.V1.Protos.WebRtcStreamer;
using Microsoft.AspNetCore.Http;
using Avalanche.Api.Managers.Security;
using Ism.Security.Grpc.Configuration;
using System.Collections.Generic;
using Ism.SystemState.Client.V1;
using Avalanche.Api.Services.Security;
using System.IdentityModel.Tokens.Jwt;
using Ism.Common.Core.Configuration.Extensions;

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
            services.AddMvc().AddNewtonsoftJson();

            services.AddCustomSwagger();
            services.AddHttpContextAccessor();

            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = 209715200;
            });

            IConfigurationService configurationService = new ConfigurationService(Configuration);
            services.AddSingleton(c => configurationService);

            // needed for state client and maybe others
            services.AddPocoConfiguration<GrpcServiceRegistry>(Configuration, nameof(GrpcServiceRegistry));


            var grpcCertificate = configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = configurationService.GetEnvironmentVariable("grpcPassword");
            var grpcServerValidationCertificate = configurationService.GetEnvironmentVariable("grpcServerValidationCertificate");

            services.AddTransient<IRoutingManager, RoutingManager>();
            services.AddTransient<IWebRTCManager, WebRTCManager>();
            services.AddTransient<IRecordingManager, RecordingManager>();
            services.AddTransient<IMaintenanceManager, MaintenanceManager>();
            services.AddTransient<IPatientsManager, PatientsManager>();
            services.AddTransient<IDataManager, DataManager>();
            services.AddTransient<ILicensingManager, LicensingManagerMock>();
            services.AddTransient<IProceduresManager, ProceduresManager>();
            services.AddTransient<INotificationsManager, NotificationsManager>();
            services.AddTransient<ISecurityManager, SecurityManager>();

            //Don't change this, this need to be Singleton due to its behavior, until a good architecture will be applied
            services.AddSingleton<IPgsTimeoutManager, PgsTimeoutManager>();

            services.AddSingleton<IWebRTCService, WebRTCService>();
            services.AddSingleton<IRecorderService, RecorderService>();
            services.AddSingleton<IAvidisService, AvidisService>();
            services.AddSingleton<IRoutingService, RoutingService>();
            services.AddSingleton<IPgsTimeoutService, PgsTimeoutService>();
            services.AddSingleton<IPieService, PieService>();
            services.AddSingleton<IBroadcastService, BroadcastService>();
            services.AddSingleton<IStorageService, StorageService>();
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
            services.AddSingleton<ICookieValidationService, CookieValidationService>();

            services.AddHostedService<NotificationsListener>();

            services.AddAutoMapper(typeof(Startup));

            var stateServiceAddress = configurationService.GetEnvironmentVariable("stateServiceGrpcAddress");
            var stateServicePort = configurationService.GetEnvironmentVariable("stateServiceGrpcPort");
            services.AddGrpcStateClient("AvalancheApi");

            ConfigureAuthorization(services);
            ConfigureCorsPolicy(services);
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            services.Configure<TokenOptions>(Configuration.GetSection("TokenOptions"));
            var tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

            services.Configure<AuthSettings>(Configuration.GetSection("AuthSettings"));
            var authSettings = Configuration.GetSection("AuthSettings").Get<AuthSettings>();

            services.Configure<CookieSettings>(Configuration.GetSection("CookieSettings"));
            var cookieSettings = Configuration.GetSection("CookieSettings").Get<CookieSettings>();

            var signingConfigurations = new SigningConfigurations(authSettings.SecretKey);
            services.AddSingleton(signingConfigurations);

            var rootPath = Configuration.GetSection("hostingRootPath")?.Value ?? "/";

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = JwtUtilities.GetDefaultJwtValidationParameters(tokenOptions, signingConfigurations);

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
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, cookieOptions =>
            {
                cookieOptions.Cookie.HttpOnly = true;
                cookieOptions.Cookie.IsEssential = true;
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookieOptions.Cookie.SameSite = SameSiteMode.Strict;
                cookieOptions.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSettings.ExpirationSeconds);

                cookieOptions.Cookie.Path = rootPath + cookieSettings.Path;
                cookieOptions.LoginPath = "/login"; // this is route to angular app login page

                // forward anything not to the cookie path to the jwt auth handler
                cookieOptions.ForwardDefaultSelector = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments(cookieSettings.Path, StringComparison.OrdinalIgnoreCase)) 
                    { 
                        return null;
                    }
                    return JwtBearerDefaults.AuthenticationScheme;
                };

                cookieOptions.EventsType = typeof(AvalancheCookieAuthenticationEvents);
            });

            services.AddScoped<AvalancheCookieAuthenticationEvents>();
            services.AddSingleton<ICookieValidationService, CookieValidationService>();

            services.AddAuthorization(options =>
            {
                var builder = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser();
                options.DefaultPolicy = builder.Build();
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
