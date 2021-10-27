using System;
using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;
using Grpc.Core;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Avalanche.Api.TempExtensions
{
    // TEMP Fix until changes are moved back to Ism.Security
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalAndRemoteSecureGrpcClient<TSecureClient, TGrpcClient>(this IServiceCollection services, string localServiceName, string remoteServiceName)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var namedServices = new List<NamedService<TSecureClient, TGrpcClient>>();

            _ = services.AddSingleton<IGrpcClientFactory<TGrpcClient>, GrpcClientFactory<TGrpcClient>>();

            _ = services.AddSingleton(sp =>
            {
                var remoteClient = CreateSecureClient<TSecureClient, TGrpcClient>(sp, remoteServiceName,
                new HostPort()
                {
                    Host = sp.GetRequiredService<VaultStreamServerConfiguration>().ServerHost,
                });

                var deviceClient = CreateSecureClient<TSecureClient, TGrpcClient>(sp, localServiceName);

                // Change NamedService to NamedClient?  Else move to common, if it's just a generic named service resolution
                // Change NamedService to take GrpcClientType

                namedServices.Add(new NamedService<TSecureClient, TGrpcClient>("Local", deviceClient));
                namedServices.Add(new NamedService<TSecureClient, TGrpcClient>("Remote", remoteClient));

                _ = services.AddSingleton(deviceClient);
                _ = services.AddSingleton(remoteClient);

                return new NamedServiceFactory<TSecureClient, TGrpcClient>(namedServices);
            });

            return services;
        }

        public static IServiceCollection AddNamedSecureGrpcClient<TSecureClient, TGrpcClient>(this IServiceCollection services, string serviceName, string key)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var namedServices = new List<NamedService<TSecureClient, TGrpcClient>>();

            _ = services.AddSingleton<IGrpcClientFactory<TGrpcClient>, GrpcClientFactory<TGrpcClient>>();

            _ = services.AddSingleton(sp =>
            {
                var deviceClient = CreateSecureClient<TSecureClient, TGrpcClient>(sp, serviceName);

                namedServices.Add(new NamedService<TSecureClient, TGrpcClient>(key, deviceClient));

                _ = services.AddSingleton(deviceClient);

                return new NamedServiceFactory<TSecureClient, TGrpcClient>(namedServices);
            });

            return services;
        }

        private static TSecureClient CreateSecureClient<TSecureClient, TGrpcClient>(IServiceProvider sp, string serviceName)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var serviceRegistry = sp.GetRequiredService<GrpcServiceRegistry>();
            var hostPort = serviceRegistry.GetServiceAddress(serviceName);

            return CreateSecureClient<TSecureClient, TGrpcClient>(sp, hostPort);
        }

        private static TSecureClient CreateSecureClient<TSecureClient, TGrpcClient>(IServiceProvider sp, string serviceName, HostPort hostPort)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var serviceRegistry = sp.GetRequiredService<GrpcServiceRegistry>();
            var defaultHostPort = serviceRegistry.GetServiceAddress(serviceName);

            hostPort.Port = defaultHostPort.Port;

            return CreateSecureClient<TSecureClient, TGrpcClient>(sp, hostPort);
        }

        private static TSecureClient CreateSecureClient<TSecureClient, TGrpcClient>(IServiceProvider sp, HostPort hostPort)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var grpcFactory = sp.GetRequiredService<IGrpcClientFactory<TGrpcClient>>();
            var certProvider = sp.GetRequiredService<ICertificateProvider>();

            return (TSecureClient)Activator.CreateInstance(typeof(TSecureClient), grpcFactory, hostPort, certProvider);
        }
    }
}
