using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ism.Common.Core.Configuration.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static Ism.Common.Core.Hosting.HostingUtilities;


namespace Avalanche.Security.Server
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
                var host = CreateIsmHostBuilder<Startup>(
                    args,
                    hostLogger,
                    typeof(Program).Assembly,
                    GetConfigurationServiceRequests(),
                    null
                ).Build();

                var loggerFactory = InitializeApplicationLoggerFactory(host);

                // Overwrite the default hostLogger with one from DI so we can get better logging if there is a failure calling host.Run()
                hostLogger = loggerFactory.CreateLogger(nameof(Program));
                host.Run();
            }
            catch (Exception ex)
            {
                hostLogger.LogError(ex, "ServiceHost host terminated unexpectedly");
                throw;
            }
        }

        private static IEnumerable<ConfigurationServiceRequest> GetConfigurationServiceRequests()
        {
            var context = ConfigurationContext.FromEnvironment();
            return new List<ConfigurationServiceRequest>();
        }
    }
}

