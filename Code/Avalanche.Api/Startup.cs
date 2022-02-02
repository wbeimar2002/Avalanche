using System.Diagnostics.CodeAnalysis;
using Avalanche.Api.Extensions;
using Avalanche.Api.Handlers;
using Avalanche.Api.Handlers.Security.Hashing;
using Avalanche.Api.Handlers.Security.Tokens;
using Avalanche.Api.Hubs;
using Avalanche.Api.Managers;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Managers.Notifications;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Managers.Presets;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Managers.Security;
using Avalanche.Api.Options;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Medpresence;
using Avalanche.Api.Services.Notifications;
using Avalanche.Api.Services.Printing;
using Avalanche.Api.Services.Security;
using Avalanche.Api.TempExtensions;
using Avalanche.Api.Utilities;
using Avalanche.Security.Server.Client.V1.Extensions;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Options;
using AvidisDeviceInterface.Client.V1;
using Ism.Broadcaster.Services;
using Ism.Common.Core.Configuration.Extensions;
using Ism.Common.Core.Extensions;
using Ism.Common.Core.Hosting.Configuration;
using Ism.Library.Client.V1;
using Ism.Medpresence.Client.V1.Extensions;
using Ism.PatientInfoEngine.Client.V1.Extensions;
using Ism.PgsTimeout.Client.V1;
using Ism.PrintServer.Client.V1;
using Ism.Recorder.Client.V1;
using Ism.Routing.Client.V1;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Interfaces;
using Ism.Storage.Configuration.Client.V1.Extensions;
using Ism.Storage.DataManagement.Client.V1.Extensions;
using Ism.Storage.PatientList.Client.V1.Extensions;
using Ism.Streaming.Client.V1;
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
using Microsoft.FeatureManagement;
using Serilog;

using static Ism.PrintServer.V1.Protos.PrintServer;

namespace Avalanche.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ASP.NET Features
            services.AddFeatureManagement()
                .UseDisabledFeaturesHandler(new DisabledFeatureHandler());

            var isDevice = IsDevice(services);

            services.AddControllers();
            services.AddSignalR();
            services.AddMvc().AddNewtonsoftJson();
            services.AddHttpContextAccessor();
            services.AddCustomSwagger();
            services.AddAutoMapper(typeof(Startup));
            services.Configure<FormOptions>(x => x.MultipartBodyLengthLimit = 209715200);

            ConfigureAuthorization(services);
            ConfigureCorsPolicy(services);

            // Configuration
            services.AddConfigurationLoggingOnStartup();

            // Transient
            if (isDevice)
            {
                services.AddTransient<IDataManager, DeviceDataManager>();
                services.AddTransient<IRoutingManager, RoutingManager>();
                services.AddTransient<IWebRTCManager, WebRTCManager>();
                services.AddTransient<IRecordingManager, RecordingManager>();
                services.AddSingleton<IPgsTimeoutManager, PgsTimeoutManager>(); //We need this as singleton
                services.AddTransient<IActiveProcedureManager, ActiveProcedureManager>();

                services.AddTransient<IConfigurationManager, DeviceConfigurationManager>();
                services.AddTransient<IMaintenanceManager, DeviceMaintenanceManager>();
            }
            else
            {
                services.AddTransient<IDataManager, ServerDataManager>();
                services.AddTransient<IConfigurationManager, ServerConfigurationManager>();
                services.AddTransient<IMaintenanceManager, ServerMaintenanceManager>();
            }

            //Shared
            services.AddTransient<IAuthenticationManager, AuthenticationManager>();
            services.AddTransient<ISharedConfigurationManager, SharedConfigurationManager>();
            services.AddTransient<IFilesManager, FilesManager>();
            services.AddTransient<IPatientsManager, PatientsManager>();
            services.AddTransient<IProceduresManager, ProceduresManager>();
            services.AddTransient<ILicensingManager, LicensingManagerMock>();
            services.AddTransient<INotificationsManager, NotificationsManager>();
            services.AddTransient<ISecurityManager, SecurityManager>();
            services.AddTransient<IPhysiciansManager, PhysiciansManager>();
            services.AddTransient<IMedpresenceManager, MedpresenceManager>();
            services.AddTransient<IUsersManager, UsersManager>();

            // Singleton
            if (isDevice)
            {
                services.AddConfigurationPoco<PgsApiConfiguration>(_configuration, nameof(PgsApiConfiguration));
                services.AddConfigurationPoco<TimeoutApiConfiguration>(_configuration, nameof(TimeoutApiConfiguration));
                services.AddConfigurationPoco<RecorderConfiguration>(_configuration, nameof(RecorderConfiguration));
                services.AddConfigurationPoco<AutoLabelsConfiguration>(_configuration, nameof(AutoLabelsConfiguration));
                services.AddConfigurationPoco<LabelsConfiguration>(_configuration, nameof(LabelsConfiguration));
                services.AddConfigurationPoco<VaultStreamServerConfiguration>(_configuration, nameof(VaultStreamServerConfiguration));
                services.AddConfigurationPoco<FinishOptionsConfiguration>(_configuration, nameof(FinishOptionsConfiguration));
            }

            //Shared
            services.AddConfigurationPoco<ProceduresConfiguration>(_configuration, nameof(ProceduresConfiguration));
            services.AddConfigurationPoco<SetupConfiguration>(_configuration, nameof(SetupConfiguration));
            services.AddConfigurationPoco<PrintingConfiguration>(_configuration, nameof(PrintingConfiguration));
            services.AddConfigurationPoco<MedPresenceConfiguration>(_configuration, nameof(MedPresenceConfiguration));

            if (isDevice)
            {
                services.AddSingleton<IWebRTCService, WebRtcService>();
                services.AddSingleton<IRecorderService, RecorderService>();
                services.AddSingleton<IAvidisService, AvidisService>();
                services.AddSingleton<IRoutingService, RoutingService>();
                services.AddSingleton<IPgsTimeoutService, PgsTimeoutService>();
            }

            //Shared
            services.AddTransient<IPrintingService, PrintingService>(); //This needs to be transient because the constructor has a runtime condition
            services.AddSingleton<IPieService, PieService>();
            services.AddSingleton<IBroadcastService, BroadcastService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IDataManagementService, DataManagementService>();
            services.AddSingleton<ILibraryService, LibraryService>();
            services.AddSingleton<IAccessInfoFactory, AccessInfoFactory>();
            services.AddSingleton<IFilesService, FilesService>();
            services.AddSingleton<IPresetManager, PresetManager>();
            services.AddSingleton<IMedpresenceService, MedpresenceService>();
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenHandler, TokenHandler>();

        // gRPC Infrastructure
        _ = services.AddConfigurationPoco<GrpcServiceRegistry>(_configuration, nameof(GrpcServiceRegistry));
            _ = services.AddConfigurationPoco<HostingConfiguration>(_configuration, nameof(HostingConfiguration));
            _ = services.AddConfigurationPoco<ClientCertificateConfiguration>(_configuration, nameof(ClientCertificateConfiguration));

            _ = services.AddSingleton<ICertificateProvider, FileSystemCertificateProvider>();

            // gRPC Clients
            _ = services.AddMedpresenceSecureClient();
            _ = services.AddDataManagementStorageSecureClient();

            _ = services.AddLibrarySearchServiceSecureClient();
            _ = services.AddLibraryActiveProcedureServiceSecureClient();
            _ = services.AddLibraryManagerServiceSecureClient();

            _ = services.AddSecurityServiceClient();
            _ = services.AddPatientListStorageSecureClient();
            _ = services.AddPatientListSecureClient();

            _ = services.AddRecorderSecureClient();
            _ = services.AddGrpcStateClient("AvalancheApi"); //TODO: This name should be configurable

            _ = services.AddConfigurationServiceSecureClient();

            if (isDevice)
            {
                _ = services.AddWebRtcStreamerSecureClient();
                _ = services.AddAvidisSecureClient();

                _ = services.AddPgsTimeoutSecureClient();
                _ = services.AddRoutingSecureClient();

                //For printing both Services can be used according to a configuration
                //_ = services.AddPrintingServerSecureClients();

                // TEMP Fix until changes are moved back to Ism.Security and Print
                _ = services.AddLocalAndRemoteSecureGrpcClient<PrintingServerSecureClient, PrintServerClient>("PrintServer", "PrintServerVSS");

                // Hosted Services
                services.AddHostedService<DeviceNotificationsListener>();
            }
            else
            {
                _ = services.AddNamedSecureGrpcClient<PrintingServerSecureClient, PrintServerClient>("PrintServer", "Local");

                // Hosted Services
                services.AddHostedService<ServerNotificationsListener>();
            }
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            // Add configuration for Auth
            services.AddConfigurationPoco<TokenAuthConfiguration>(_configuration, nameof(TokenAuthConfiguration));
            services.AddConfigurationPoco<AuthConfiguration>(_configuration, nameof(AuthConfiguration));
            services.AddConfigurationPoco<CookieAuthConfiguration>(_configuration, nameof(CookieAuthConfiguration));
            services.AddSingleton(sp => new SigningOptions(sp.GetRequiredService<AuthConfiguration>().SecretKey));

            // Configure
            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //CookieAuthenticationDefaults.AuthenticationScheme; ??? This will affect?
            })
            .AddJwtBearer()
            .AddCookie();

            services.ConfigureOptions<ConfigureJwtBearerOptions>();
            services.ConfigureOptions<ConfigureCookieAuthenticiationOptions>();

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
                        .WithOrigins("https://localhost:4200", "http://localhost:4200", "http://localhost:8080", "http://localhost:8082")
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

        private bool IsDevice(IServiceCollection services)
        {
            using var provider = services.BuildServiceProvider();
            var featureManager = provider.GetRequiredService<IFeatureManager>();

            return featureManager.IsEnabledAsync(FeatureFlags.IsDevice).Result;
        }
    }
}
