using Avalanche.Host.Service.Clients;
using Avalanche.Host.Service.Enumerations;
using Avalanche.Host.Service.Helpers;
using Avalanche.Host.Service.Services.Logging;
using Avalanche.Host.Service.Services.Security;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            IAppLoggerService logger = null;

            try
            {
                Head.Register<AuthorizationServiceProto.AuthorizationServiceProtoBase, AuthorizationService>();
                Head.Register<ISecurityService, SecurityService>();

                var svr = new Server
                {
                    Services = {
                        AuthorizationServiceProto.BindService(Head.GetImplementation<AuthorizationServiceProto.AuthorizationServiceProtoBase>()),
                    },
                    Ports = { new ServerPort(_hostname, _port, ServerCredentials.Insecure) }
                };

                svr.Start();

                logger = Head.GetImplementation<IAppLoggerService>();
                logger.Log(LogType.Information, "Avalanche Host Service is now initialized.");
            }
            catch (Exception ex)
            {
                if (logger == null) { throw; }
                else { logger.Log(LogType.Fatal, "Avalanche Host Service failed to initialize!", ex); }
            }
        }
    }
}