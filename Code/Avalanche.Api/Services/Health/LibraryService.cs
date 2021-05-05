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
        private readonly LibraryServiceSecureClient _serviceClient;

        public LibraryService(LibraryActiveProcedureServiceSecureClient activeClient, LibraryServiceSecureClient serviceClient)
        {
            _activeClient = activeClient;
            _serviceClient = serviceClient;
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

        public async Task DeleteActiveProcedureMedia(DeleteActiveProcedureMediaRequest deleteActiveProcedureMediaRequest)
        {
            await _activeClient.DeleteActiveProcedureMedia(deleteActiveProcedureMediaRequest);
        }

        public async Task<ReindexRepositoryResponse> ReindexRepository(string repositoryName)
        {
            return await _serviceClient.ReindexRepository(new ReindexRepositoryRequest()
            {
                RepositoryName = repositoryName
            });
        }
    }
}
