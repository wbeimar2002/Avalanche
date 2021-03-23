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

namespace Avalanche.Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Create a simple hostLogger to log an exception during or before calling host.Build()
            var hostLogger = CreateDefaultHostLogger(typeof(Program));
            try
            {
                var configClient = ConfigurationServiceSecureClient.FromEnvironment(hostLogger);
                var host = CreateInsecureIsmHostBuilder<Startup>(
                    args,
                    hostLogger,
                    typeof(Program).Assembly,
                    GetConfigurationServiceRequests(),
                    configClient,
                    HttpProtocols.Http1AndHttp2
                )
                .Build();

                var loggerFactory = InitializeApplicationLoggerFactory(host);

                // Overwrite the default hostLogger with one from DI so we can get better logging if there is a failure calling host.Run()
                hostLogger = loggerFactory.CreateLogger(nameof(Program));
                host.Run();
            }
            catch (Exception ex)
            {
                hostLogger.LogError(ex, $"{nameof(Avalanche)}.{nameof(Api)} host terminated unexpectedly");
                throw;
            }
        }

        private static IEnumerable<ConfigurationServiceRequest> GetConfigurationServiceRequests()
        {
            // config types may be decorated with guid and version and other helper attributes
            var context = new ConfigurationContext();
            var requests = new List<ConfigurationServiceRequest>
            {
                // needed for grpc clients
                // new ConfigurationServiceRequest(nameof(GrpcServiceRegistry), 1, context)
            };

            return requests;
        }
    }
}
