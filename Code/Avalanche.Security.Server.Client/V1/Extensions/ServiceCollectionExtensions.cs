using Ism.Security.Grpc.Extensions;
using Microsoft.Extensions.DependencyInjection;
using static Avalanche.Security.Server.Client.V1.Protos.UsersManagement;

namespace Avalanche.Security.Server.Client.V1.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUsersManagementServiceClient(this IServiceCollection services, string serviceName = UsersManagementServiceClient.ServiceName) =>
        services.AddSecureGrpcClient<UsersManagementServiceClient, UsersManagementClient>(serviceName);
    }
}
