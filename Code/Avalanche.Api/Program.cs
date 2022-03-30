using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Configuration.Client.V1;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using static Ism.Common.Core.Hosting.HostingUtilities;
using Ism.Common.Core.Configuration;
using System.Threading.Tasks;

namespace Avalanche.Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Create a simple hostLogger to log an exception during or before calling host.Build()
            var hostLogger = CreateDefaultHostLogger(typeof(Program));
            try
            {
                var host = CreateHostBuilder(args).Build();

                var loggerFactory = InitializeApplicationLoggerFactory(host);

                // Overwrite the default hostLogger with one from DI so we can get better logging if there is a failure calling host.Run()
                hostLogger = loggerFactory.CreateLogger(nameof(Program));
                await host.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                hostLogger.LogError(ex, $"{nameof(Avalanche)}.{nameof(Api)} host terminated unexpectedly");
                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostLogger = CreateDefaultHostLogger(typeof(Program));
            var configClient = ConfigurationServiceSecureClient.FromEnvironment(hostLogger);

            return CreateIsmHostBuilder<Startup>(
                    args,
                    hostLogger,
                    typeof(Program).Assembly,
                    GetConfigurationServiceRequests(),
                    configClient,
                    false,
                    HttpProtocols.Http1AndHttp2
                );
        }

        private static IEnumerable<ConfigurationServiceRequest> GetConfigurationServiceRequests()
        {
            // config types may be decorated with guid and version and other helper attributes
            var context = ConfigurationContext.FromEnvironment();
            return new List<ConfigurationServiceRequest>
            {
                new ConfigurationServiceRequest(nameof(PgsApiConfiguration), 1, context),
                new ConfigurationServiceRequest(nameof(TimeoutApiConfiguration), 1, context),
                new ConfigurationServiceRequest(nameof(RecorderConfiguration), 1, context),
                ConfigurationServiceRequestFactory.CreateRequest<WebRtcApiConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<SetupConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<ProceduresConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<AutoLabelsConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<LabelsConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<PrintingConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<MedPresenceConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<VaultStreamServerConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<FinishOptionsConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<MedPresenceProvisioningConfiguration>(context),
                ConfigurationServiceRequestFactory.CreateRequest<AutoLoginConfiguration>(context)
            };
        }
    }
}
