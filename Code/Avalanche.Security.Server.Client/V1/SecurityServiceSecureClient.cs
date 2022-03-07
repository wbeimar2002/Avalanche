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

        public async Task<AddUserResponse> AddUser(AddUserRequest request) =>
           await Client.AddUserAsync(request);

        public async Task DeleteUser(DeleteUserRequest request) =>
           _ = await Client.DeleteUserAsync(request);

        public async Task<GetUserResponse> GetUser(GetUserRequest request) =>
           await Client.GetUserAsync(request);

        public async Task<GetUsersResponse> GetUsers() =>
            await Client.GetUsersAsync(new Empty());

        public async Task<SearchUsersResponse> SearchUsers(SearchUsersRequest request) =>
           await Client.SearchUsersAsync(request);

        public async Task UpdateUser(UpdateUserRequest request) =>
            _ = await Client.UpdateUserAsync(request);

        public async Task UpdateUserPassword(UpdateUserPasswordRequest request) =>
            _ = await Client.UpdateUserPasswordAsync(request);

        public async Task<VerifyUserLoginResponse> VerifyUserLogin(VerifyUserLoginRequest request) =>
            await Client.VerifyUserLoginAsync(request);

        public async Task<VerifyUserLoginResponse> VerifyAdminUserLoginAsync(VerifyUserLoginRequest request) =>
            await Client.VerifyAdminUserLoginAsync(request);
    }
}
