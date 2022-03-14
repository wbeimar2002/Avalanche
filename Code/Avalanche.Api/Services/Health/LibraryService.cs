using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ism.Library.Client.V1;
using Ism.Library.V1.Protos;

using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Security.Grpc;
using Microsoft.FeatureManagement;
using static Ism.Library.V1.Protos.LibraryActiveProcedureService;
using static Ism.Library.V1.Protos.LibraryManagerService;
using static Ism.Library.V1.Protos.LibrarySearchService;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class LibraryService : ILibraryService
    {
        private readonly LibraryActiveProcedureServiceSecureClient _libraryActiveProcedureServiceSecureClient;

        private readonly LibraryManagerServiceSecureClient _libraryManagerServiceSecureClient;
        private readonly LibrarySearchServiceSecureClient _librarySearchServiceSecureClient;

        private readonly LibraryManagerServiceSecureClient _libraryManagerServiceSecureClientRemote;
        private readonly LibrarySearchServiceSecureClient _librarySearchServiceSecureClientRemote;


        private readonly bool _isDevice = false;

        public LibraryService(
            NamedServiceFactory<LibraryActiveProcedureServiceSecureClient, LibraryActiveProcedureServiceClient> activeProcedureFactory,
            NamedServiceFactory<LibraryManagerServiceSecureClient, LibraryManagerServiceClient> managerFactory,
            NamedServiceFactory<LibrarySearchServiceSecureClient, LibrarySearchServiceClient> searchFactory,
            IFeatureManager featureManager)
        {
            _isDevice = featureManager.IsEnabledAsync(FeatureFlags.IsDevice).Result;

            if (_isDevice)
            {
                _libraryActiveProcedureServiceSecureClient = activeProcedureFactory.GetClient("Local");
                _libraryManagerServiceSecureClient = managerFactory.GetClient("Local");
                _librarySearchServiceSecureClient = searchFactory.GetClient("Local");

                _libraryManagerServiceSecureClientRemote = managerFactory.GetClient("Remote");
                _librarySearchServiceSecureClientRemote = searchFactory.GetClient("Remote");
            }
            else
            {
                _libraryManagerServiceSecureClient = managerFactory.GetClient("Local");
                _librarySearchServiceSecureClient = searchFactory.GetClient("Local");
            }
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

        //Manager

        public async Task DownloadProcedure(ProcedureDownloadRequest procedureDownloadRequest) =>
            await _libraryManagerServiceSecureClient.DownloadProcedure(procedureDownloadRequest).ConfigureAwait(false);

        public async Task UpdateProcedure(UpdateProcedureRequest updateProcedureRequest) =>
            await _libraryManagerServiceSecureClient.UpdateProcedure(updateProcedureRequest).ConfigureAwait(false);

        public async Task ShareProcedure(ShareProcedureRequest shareProcedureRequest) =>
            await _libraryManagerServiceSecureClient.ShareProcedure(shareProcedureRequest).ConfigureAwait(false);

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
