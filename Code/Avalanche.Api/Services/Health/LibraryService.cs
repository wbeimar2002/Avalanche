using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Library.Client.Core.V1;
using Ism.Library.Core.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.Library.Core.V1.Protos.LibraryService;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class LibraryService : ILibraryService
    {
        LibraryServiceSecureClient _libraryServiceClient { get; set; }

        public LibraryService(IConfigurationService configurationService, IGrpcClientFactory<LibraryServiceClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            var hostIpAddress = configurationService.GetEnvironmentVariable("hostIpAddress");
            var libraryServiceGrpcPort = configurationService.GetEnvironmentVariable("libraryServiceGrpcPort");

            _libraryServiceClient = new LibraryServiceSecureClient(grpcClientFactory, hostIpAddress, libraryServiceGrpcPort, certificateProvider);
        }

        public async Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest)
        {
            await _libraryServiceClient.DiscardActiveProcedure(discardActiveProcedureRequest);
        }

        public async Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest)
        {
            await _libraryServiceClient.CommitActiveProcedure(commitActiveProcedureRequest);
        }

        public async Task<AllocateNewProcedureResponse> CommitActiveProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest)
        {
            return await _libraryServiceClient.AllocateNewProcedure(allocateNewProcedureRequest);
        }

        public async Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest request)
        {
            return await _libraryServiceClient.AllocateNewProcedure(request);
        }
    }
}
