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
    public class SharedLibraryService
    {
        private readonly LibraryManagerServiceSecureClient _libraryManagerServiceSecureClient;
        private readonly LibrarySearchServiceSecureClient _librarySearchServiceSecureClient;

        public SharedLibraryService(
            NamedServiceFactory<LibraryManagerServiceSecureClient, LibraryManagerServiceClient> managerFactory,
            NamedServiceFactory<LibrarySearchServiceSecureClient, LibrarySearchServiceClient> searchFactory)
        {
            _libraryManagerServiceSecureClient = managerFactory.GetClient("Local");
            _librarySearchServiceSecureClient = searchFactory.GetClient("Local");
        }

        //Manager

        public async Task DownloadProcedure(ProcedureDownloadRequest procedureDownloadRequest) =>
            await _libraryManagerServiceSecureClient.DownloadProcedure(procedureDownloadRequest).ConfigureAwait(false);

        public async Task UpdateProcedure(UpdateProcedureRequest updateProcedureRequest) =>
            await _libraryManagerServiceSecureClient.UpdateProcedure(updateProcedureRequest).ConfigureAwait(false);

        public async Task ShareProcedure(ShareProcedureRequest shareProcedureRequest) =>
            await _libraryManagerServiceSecureClient.ShareProcedure(shareProcedureRequest).ConfigureAwait(false);

        public async Task<ReindexRepositoryResponse> ReindexRepository(ReindexRepositoryRequest reindexRepositoryRequest) =>
            await _libraryManagerServiceSecureClient.ReindexRepository(reindexRepositoryRequest).ConfigureAwait(false);

        //Search

        public async Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest) =>
            await _librarySearchServiceSecureClient.GetFinishedProcedure(getFinishedProcedureRequest).ConfigureAwait(false);

        public async Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest) =>
            await _librarySearchServiceSecureClient.GetFinishedProcedures(getFinishedProceduresRequest).ConfigureAwait(false);

        public async Task<GetFinishedProceduresResponse> GetFinishedProceduresByPatient(GetFinishedProceduresRequestByPatient getFinishedProceduresRequestByPatient) =>
            await _librarySearchServiceSecureClient.GetFinishedProceduresByPatient(getFinishedProceduresRequestByPatient).ConfigureAwait(false);

        public async Task<GetPhysiciansSearchResponse> GetPhysicians(GetPhysiciansSearchRequest getPhysiciansSearchRequest) =>
            await _librarySearchServiceSecureClient.GetPhysicians(getPhysiciansSearchRequest).ConfigureAwait(false);
    }
}
