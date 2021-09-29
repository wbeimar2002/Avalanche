using System;
using System.Collections.Generic;
using Avalanche.Api.Services.Printing;
using Ism.PrintServer.Client.V1;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using static Ism.PrintServer.Client.PrintServer;

namespace Avalanche.Api.Extensions
{
    public static class PrintingExtensions
    {
        private const string PRINT_SERVER_KEY = "PrintServer";
        private const string PRINT_SERVER_VSS_KEY = "PrintServerVSS";

        public static IServiceCollection AddPrintingServerSecureClients(this IServiceCollection services)
        {
            var namedPrintServers = new List<NamedPrintServer>();

            services.AddSingleton<IGrpcClientFactory<PrintServerClient>, GrpcClientFactory<PrintServerClient>>();

            using var sp = services.BuildServiceProvider();

            var grpcFactory = sp.GetRequiredService<IGrpcClientFactory<PrintServerClient>>();
            var certProvider = sp.GetRequiredService<ICertificateProvider>();

            var serviceRegistry = sp.GetRequiredService<GrpcServiceRegistry>();

            var hostPortDevice = serviceRegistry.GetServiceAddress(PRINT_SERVER_KEY);

            var deviceClient = (PrintingServerSecureClient)Activator.CreateInstance(typeof(PrintingServerSecureClient), grpcFactory, hostPortDevice, certProvider);
            namedPrintServers.Add(new NamedPrintServer(PRINT_SERVER_KEY, deviceClient));
            services.AddSingleton(deviceClient);

            var hostPortVSS = serviceRegistry.GetServiceAddress(PRINT_SERVER_VSS_KEY);

            var vssClient = (PrintingServerSecureClient)Activator.CreateInstance(typeof(PrintingServerSecureClient), grpcFactory, hostPortVSS, certProvider);
            namedPrintServers.Add(new NamedPrintServer(PRINT_SERVER_VSS_KEY, vssClient));
            services.AddSingleton(vssClient);

            services.AddSingleton(sp => new PrintServerFactory(namedPrintServers));

            return services;
        }
    }
}
