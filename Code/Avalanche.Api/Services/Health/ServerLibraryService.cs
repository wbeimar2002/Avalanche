using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ism.Library.Client.V1;
using Ism.Library.V1.Protos;
using Ism.Security.Grpc;
using static Ism.Library.V1.Protos.LibraryManagerService;
using static Ism.Library.V1.Protos.LibrarySearchService;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class ServerLibraryService : SharedLibraryService, ILibraryService
    {
        public ServerLibraryService(
            NamedServiceFactory<LibraryManagerServiceSecureClient, LibraryManagerServiceClient> managerFactory,
            NamedServiceFactory<LibrarySearchServiceSecureClient, LibrarySearchServiceClient> searchFactory) :
            base (managerFactory, searchFactory)
        {
        }

        public Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest) => throw new System.NotImplementedException();
        public Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest) => throw new System.NotImplementedException();
        public Task DeleteActiveProcedureMediaItem(DeleteActiveProcedureMediaItemRequest deleteActiveProcedureMediaItemRequest) => throw new System.NotImplementedException();
        public Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest) => throw new System.NotImplementedException();
        public Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest) => throw new System.NotImplementedException();
    }
}
