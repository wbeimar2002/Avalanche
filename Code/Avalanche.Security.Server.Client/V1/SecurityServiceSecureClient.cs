using System.Threading.Tasks;
using Avalanche.Security.Server.Client.V1.Protos;
using Google.Protobuf.WellKnownTypes;
using Ism.Security.Grpc;
using Ism.Security.Grpc.Configuration;
using Ism.Security.Grpc.Interfaces;
using static Avalanche.Security.Server.Client.V1.Protos.Security;

namespace Avalanche.Security.Server.Client.V1
{
    public class SecurityServiceSecureClient : SecureClientBase<SecurityClient>
    {
        public const string ServiceName = "Security";

        public SecurityServiceSecureClient(IGrpcClientFactory<SecurityClient> factory, string host, uint port, ICertificateProvider certificateProvider)
        : base(factory, host, port, certificateProvider)
        {
        }

        public SecurityServiceSecureClient(IGrpcClientFactory<SecurityClient> factory, string host, string port, ICertificateProvider certificateProvider)
            : base(factory, host, port, certificateProvider)
        {
        }

        public SecurityServiceSecureClient(IGrpcClientFactory<SecurityClient> factory, HostPort hostPort, ICertificateProvider certificateProvider)
            : base(factory, hostPort, certificateProvider)
        {
        }

        public async Task<AddUserResponse> AddUserAsync(AddUserRequest request) =>
           await Client.AddUserAsync(request);

        public async Task<Empty> DeleteUserAsync(DeleteUserRequest request) =>
           await Client.DeleteUserAsync(request);

        public async Task<GetUserResponse> FindByUserNameAsync(GetUserRequest request) =>
           await Client.GetUserAsync(request);

        public async Task<GetUsersResponse> GetUsersAsync() =>
            await Client.GetUsersAsync(new Empty());
        public async Task<SearchUsersResponse> SearchUsersAsync(SearchUsersRequest request) =>
           await Client.SearchUsersAsync(request);

        public async Task<Empty> UpdateUserAsync(UpdateUserRequest request) =>
            await Client.UpdateUserAsync(request);
    }
}
