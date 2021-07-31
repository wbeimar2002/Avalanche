using Ism.Library.Client.V1;
using Ism.Library.V1.Protos;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class LibraryService : ILibraryService
    {
        private readonly LibraryActiveProcedureServiceSecureClient _activeClient;
        private readonly LibraryManagerServiceSecureClient _managerServiceClient;
        private readonly LibrarySearchServiceSecureClient _serviceSearchClient;

        public LibraryService(LibraryActiveProcedureServiceSecureClient activeClient, LibraryManagerServiceSecureClient managerServiceClient, LibrarySearchServiceSecureClient serviceSearchClient)
        {
            _activeClient = activeClient;
            _managerServiceClient = managerServiceClient;
            _serviceSearchClient = serviceSearchClient;
        }

        public async Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest)
        {
            await _activeClient.DiscardActiveProcedure(discardActiveProcedureRequest);
        }

        public async Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest)
        {
            await _activeClient.CommitActiveProcedure(commitActiveProcedureRequest);
        }

        public async Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest)
        {
            return await _activeClient.AllocateNewProcedure(allocateNewProcedureRequest);
        }

        public async Task UpdateProcedure(UpdateProcedureRequest updateProcedureRequest)
        {
            await _managerServiceClient.UpdateProcedure(updateProcedureRequest);
        }

        public async Task DeleteActiveProcedureMediaItem(DeleteActiveProcedureMediaRequest deleteActiveProcedureMediaRequest)
        {
            await _activeClient.DeleteActiveProcedureMedia(deleteActiveProcedureMediaRequest);
        }

        public async Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest)
        {
            await _activeClient.DeleteActiveProcedureMediaItems(deleteActiveProcedureMediaItemsRequest);
        }

        public async Task<ReindexRepositoryResponse> ReindexRepository(string repositoryName)
        {
            return await _managerServiceClient.ReindexRepository(new ReindexRepositoryRequest()
            {
                RepositoryName = repositoryName
            });
        }

        public async Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest)
        {
            return await _serviceSearchClient.GetFinishedProcedure(getFinishedProcedureRequest);
        }

        public async Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest)
        {
            return await _serviceSearchClient.GetFinishedProcedures(getFinishedProceduresRequest);
        }
    }
}
