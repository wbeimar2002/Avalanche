﻿using Avalanche.Host.Service.Clients;
using Avalanche.Host.Service.Enumerations;
using Avalanche.Host.Service.Helpers;
using Avalanche.Host.Service.Services.Security;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using Ism.Security.Grpc.Helpers;
using Ism.Security.Grpc.Interceptors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
                //Change this values to the AppConfig
                string certificateKeyPath = @"C:\Olympus\certificates\grpc_serverl5.pem";
                string certificateFilePath = @"C:\Olympus\certificates\grpc_serverl5.crt";
                int chainLevelsToValidate = 5;

                logger = new ConsoleLogger();

                IoCHelper.Register(() => logger, IoCLifestyle.Singleton);
                IoCHelper.Register<AuthorizationServiceProto.AuthorizationServiceProtoBase, AuthorizationService>();
                IoCHelper.Register<ISecurityService, SecurityService>();

                logger = IoCHelper.GetImplementation<ILogger>();

                var authorizationServiceImplementation = IoCHelper.GetImplementation<AuthorizationServiceProto.AuthorizationServiceProtoBase>();

                ServerServiceDefinition serviceDefinition = AuthorizationServiceProto.BindService(authorizationServiceImplementation)
                           .Intercept(new AuthInterceptor());

                //Server svr = ServerHelper.GetSecureServer(_hostname, _port, certificateFilePath, certificateKeyPath, chainLevelsToValidate,
                Server svr = ServerHelper.GetInsecureServer(_hostname, _port, 
                    new List<ServerServiceDefinition>() 
                    { 
                        serviceDefinition 
                    });

                svr.Start();

                logger.Info("Avalanche Host Service is now initialized.");
            }
            catch (Exception ex)
            { 
                if (logger == null) 
                { 
                    throw; 
                }
                
                else { logger.Error($"Avalanche Host Service failed to initialize! {ex.Message}", ex); }
            }
        }
    }
}
 