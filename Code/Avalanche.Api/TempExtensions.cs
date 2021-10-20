using System;
using System.Collections.Generic;
using Grpc.Core;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.TempExtensions
{
    // TEMP Fix until changes are moved back to Ism.Security
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalAndRemoteSecureGrpcClient<TSecureClient, TGrpcClient>(this IServiceCollection services, string localServiceName, HostPort remoteHostPort)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            var namedServices = new List<NamedService<TSecureClient, TGrpcClient>>();

            // Do we need to add client's directly or just to the named service factory, unclear
            _ = services.AddSecureGrpcClient<TSecureClient, TGrpcClient>(localServiceName);
            _ = services.AddSecureGrpcClient<TSecureClient, TGrpcClient>(remoteHostPort);

            _ = services.AddSingleton(sp =>
            {
                var remoteClient = CreateSecureClient<TSecureClient, TGrpcClient>(sp, remoteHostPort);
                var deviceClient = CreateSecureClient<TSecureClient, TGrpcClient>(sp, localServiceName);

                // Change NamedService to NamedClient?  Else move to common, if it's just a generic named service resolution
                // Change NamedService to take GrpcClientType
                // Don't need to add clients to DI directly?

                namedServices.Add(new NamedService<TSecureClient, TGrpcClient>("Local", deviceClient));
                namedServices.Add(new NamedService<TSecureClient, TGrpcClient>("Remote", remoteClient));
                return new NamedServiceFactory<TSecureClient, TGrpcClient>(namedServices);
            });

            return services;
        }

        public static IServiceCollection AddSecureGrpcClient<TSecureClient, TGrpcClient>(this IServiceCollection services, string serviceName)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            ThrowIfNull(nameof(services), services);
            ThrowIfNullOrEmptyOrWhiteSpace(nameof(serviceName), serviceName);

            _ = services.AddSingleton<IGrpcClientFactory<TGrpcClient>, GrpcClientFactory<TGrpcClient>>();
            _ = services.AddSingleton(sp => CreateSecureClient<TSecureClient, TGrpcClient>(sp, serviceName));

            return services;
        }

        public static IServiceCollection AddSecureGrpcClient<TSecureClient, TGrpcClient>(this IServiceCollection services, HostPort hostPort)
            where TSecureClient : SecureClientBase<TGrpcClient>
            where TGrpcClient : ClientBase
        {
            ThrowIfNull(nameof(services), services);
            ThrowIfNull(nameof(hostPort), hostPort);

            _ = services.AddSingleton(sp => CreateSecureClient<TSecureClient, TGrpcClient>(sp, hostPort));
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
