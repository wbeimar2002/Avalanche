using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Interfaces;
using System.Threading.Tasks;
using static Avalanche.Security.Server.Client.V1.Protos.Security;

namespace Avalanche.Security.Server.Client.V1
{
    public class SecurityServiceClient : SecureClientBase<SecurityClient>
    {
        public const string ServiceName = "Security";

        public SecurityServiceClient(IGrpcClientFactory<SecurityClient> factory, string host, uint port, ICertificateProvider certificateProvider)
        : base(factory, host, port, certificateProvider)
        {
        }

        public SecurityServiceClient(IGrpcClientFactory<SecurityClient> factory, string host, string port, ICertificateProvider certificateProvider)
            : base(factory, host, port, certificateProvider)
        {
        }

        public SecurityServiceClient(IGrpcClientFactory<SecurityClient> factory, HostPort hostPort, ICertificateProvider certificateProvider)
            : base(factory, hostPort, certificateProvider)
        {
        }

        public async Task<GetUsersResponse> GetUsersAsync(Empty request) =>
           await Client.GetUsersAsync(request);

        public async Task<AddUserResponse> AddUserAsync(AddUserRequest request) =>
           await Client.AddUserAsync(request);

        public async Task<Empty> UpdateUserAsync(UpdateUserRequest request) =>
           await Client.UpdateUserAsync(request);

        public async Task<Empty> DeleteUserAsync(DeleteUserRequest request) =>
           await Client.DeleteUserAsync(request);

        public async Task<FindByUserNameResponse> FindByUserNameAsync(FindByUserNameRequest request) =>
           await Client.FindByUserNameAsync(request);
    }
}