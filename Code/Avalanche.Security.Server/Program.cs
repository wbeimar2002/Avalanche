using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;

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
                var host = CreateInsecureIsmHostBuilder<Startup>(
                    args,
                    hostLogger,
                    typeof(Program).Assembly,
                    null,
                    null,
                    HttpProtocols.Http1AndHttp2
                )
                .Build();

                var loggerFactory = InitializeApplicationLoggerFactory(host);

                // Overwrite the default hostLogger with one from DI so we can get better logging if there is a failure calling host.Run()
                hostLogger = loggerFactory.CreateLogger(nameof(Program));

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetService<SecurityDbContext>();
                    var passwordHasher = services.GetService<IPasswordHasher>();
                    DatabaseSeed.Seed(context, passwordHasher);
                }

                host.Run();
            }
            catch (Exception ex)
            {
                hostLogger.LogError(ex, $"{nameof(Avalanche.Security)}.{nameof(Server)} host terminated unexpectedly");
                throw;
            }     
        }
    }
}