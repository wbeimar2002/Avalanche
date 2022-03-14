using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ism.Library.Client.V1;
using Ism.Library.V1.Protos;

using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Security.Grpc;

using static Ism.Library.V1.Protos.LibraryActiveProcedureService;
using static Ism.Library.V1.Protos.LibraryManagerService;
using static Ism.Library.V1.Protos.LibrarySearchService;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class DeviceLibraryService : SharedLibraryService, ILibraryService
    {
        private readonly LibraryActiveProcedureServiceSecureClient _libraryActiveProcedureServiceSecureClient;

        //TODO: Pending, Development in course
        private readonly LibraryManagerServiceSecureClient _libraryManagerServiceSecureClientRemote;
        private readonly LibrarySearchServiceSecureClient _librarySearchServiceSecureClientRemote;

        public DeviceLibraryService(
            NamedServiceFactory<LibraryActiveProcedureServiceSecureClient, LibraryActiveProcedureServiceClient> activeProcedureFactory,
            NamedServiceFactory<LibraryManagerServiceSecureClient, LibraryManagerServiceClient> managerFactory,
            NamedServiceFactory<LibrarySearchServiceSecureClient, LibrarySearchServiceClient> searchFactory) :
            base(managerFactory, searchFactory)
        {
            _libraryActiveProcedureServiceSecureClient = activeProcedureFactory.GetClient("Local");
            _libraryManagerServiceSecureClientRemote = managerFactory.GetClient("Remote");
            _librarySearchServiceSecureClientRemote = searchFactory.GetClient("Remote");
        }

        public async Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest) =>
            await _libraryActiveProcedureServiceSecureClient.DiscardActiveProcedure(discardActiveProcedureRequest).ConfigureAwait(false);

        public async Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest) =>
            await _libraryActiveProcedureServiceSecureClient.CommitActiveProcedure(commitActiveProcedureRequest).ConfigureAwait(false);

        public async Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest) =>
            await _libraryActiveProcedureServiceSecureClient.AllocateNewProcedure(allocateNewProcedureRequest).ConfigureAwait(false);

        public async Task DeleteActiveProcedureMediaItem(DeleteActiveProcedureMediaItemRequest deleteActiveProcedureMediaItemRequest) =>
            await _libraryActiveProcedureServiceSecureClient.DeleteActiveProcedureMediaItem(deleteActiveProcedureMediaItemRequest).ConfigureAwait(false);

        public async Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest) =>
            await _libraryActiveProcedureServiceSecureClient.DeleteActiveProcedureMediaItems(deleteActiveProcedureMediaItemsRequest).ConfigureAwait(false);
    }
}
