using Avalanche.Host.Service.Clients;
using Avalanche.Host.Service.Enumerations;
using Avalanche.Host.Service.Helpers;
using Avalanche.Host.Service.Services.Security;
using Grpc.Core;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Avalanche.Host.Service
{
    class Program
    {
        static readonly string _hostname = ConfigurationManager.AppSettings["hostname"] ?? "localhost";
        static readonly int _port = int.Parse(ConfigurationManager.AppSettings["host_service_on_port"] ?? "9011");

        static void Main()
        {
            var rc = HostFactory.Run(x => {
                x.Service<HostService>(s => {
                    s.ConstructUsing(name => new HostService());
                    s.WhenStarted(tc => { tc.Start(); });
                    s.AfterStartingService(c => { RegisterServicesForIoC(); });
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("Avalanche - Host Service");
                x.SetDisplayName("Avalanche - Host Service");
                x.SetServiceName("Avalanche - Host Service");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }

        static void RegisterServicesForIoC()
        {
            ILogger logger = null;

            try
            {
                IoCHelper.Register<ILogger>(() => CreateLogger(), IoCLifestyle.Singleton);
                IoCHelper.Register<AuthorizationServiceProto.AuthorizationServiceProtoBase, AuthorizationService>();
                IoCHelper.Register<ISecurityService, SecurityService>();

                var svr = new Server
                {
                    Services = {
                        AuthorizationServiceProto.BindService(IoCHelper.GetImplementation<AuthorizationServiceProto.AuthorizationServiceProtoBase>()),
                    },
                    Ports = { new ServerPort(_hostname, _port, ServerCredentials.Insecure) }
                };

                svr.Start();

                logger = IoCHelper.GetImplementation<ILogger>();
                logger.Information("Avalanche Host Service is now initialized.");
            }
            catch (Exception ex)
            {
                if (logger == null) { throw; }
                else { logger.Error("Avalanche Host Service failed to initialize!", ex); }
            }
        }

        private static ILogger CreateLogger()
        {
            var logFile = Environment.GetEnvironmentVariable("LoggerFileName") ?? "avalanchehostservicelogs.txt";
            var logFolder = Environment.GetEnvironmentVariable("LoggerFolder") ?? @"C:\Olympus\AvalancheLogs";

            var logFilePath = Path.Combine(logFolder, logFile);

            Int32 logFileSizeLimit = Convert.ToInt32(Environment.GetEnvironmentVariable("LoggerFileSizeLimit") ?? "209715200");
            Int32 retainedFileCountLimit = Convert.ToInt32(Environment.GetEnvironmentVariable("LoggerRetainedFileCountLimit") ?? "5");

            //https://github.com/serilog/serilog/wiki/Configuration-Basics
            string seqUrl = Environment.GetEnvironmentVariable("seqUrl") ?? "http://localhost:5341";

            LogEventLevel level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Debug;
#endif

            ILogger appLogger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .MinimumLevel.Debug()
#endif
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileCountLimit,
                    fileSizeLimitBytes: logFileSizeLimit,
                    restrictedToMinimumLevel: level)
                .WriteTo.Seq(seqUrl)
                .WriteTo.Console(restrictedToMinimumLevel: level)
                .CreateLogger();

            return appLogger;
        }

    }
}