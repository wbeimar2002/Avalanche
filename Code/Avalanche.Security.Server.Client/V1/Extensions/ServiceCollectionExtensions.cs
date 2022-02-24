using Ism.Security.Grpc.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static Avalanche.Security.Server.Client.V1.Protos.Security;

namespace Avalanche.Security.Server.Client.V1.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSecurityServiceClient(this IServiceCollection services, string serviceName = SecurityServiceSecureClient.ServiceName) =>
        services.AddSecureGrpcClient<SecurityServiceSecureClient, SecurityClient>(serviceName);
    }
}
