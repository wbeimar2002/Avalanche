using Ism.Library.Client.V1;
using Ism.Library.V1.Protos;
using System.Collections.Generic;
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

        public async Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest) =>
            await _activeClient.DiscardActiveProcedure(discardActiveProcedureRequest).ConfigureAwait(false);

        public async Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest) =>
            await _activeClient.CommitActiveProcedure(commitActiveProcedureRequest).ConfigureAwait(false);

        public async Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest) =>
            await _activeClient.AllocateNewProcedure(allocateNewProcedureRequest).ConfigureAwait(false);

        public async Task UpdateProcedure(UpdateProcedureRequest updateProcedureRequest) =>
            await _managerServiceClient.UpdateProcedure(updateProcedureRequest).ConfigureAwait(false);

        public async Task DeleteActiveProcedureMediaItem(DeleteActiveProcedureMediaItemRequest deleteActiveProcedureMediaItemRequest) =>
            await _activeClient.DeleteActiveProcedureMediaItem(deleteActiveProcedureMediaItemRequest).ConfigureAwait(false);

        public async Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest) =>
            await _activeClient.DeleteActiveProcedureMediaItems(deleteActiveProcedureMediaItemsRequest).ConfigureAwait(false);

        public async Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest) =>
            await _serviceSearchClient.GetFinishedProcedure(getFinishedProcedureRequest).ConfigureAwait(false);

        public async Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest) =>
            await _serviceSearchClient.GetFinishedProcedures(getFinishedProceduresRequest).ConfigureAwait(false);

        public async Task<GetFinishedProceduresResponse> GetFinishedProceduresByPatient(GetFinishedProceduresRequestByPatient getFinishedProceduresRequestByPatient) =>
            await _serviceSearchClient.GetFinishedProceduresByPatient(getFinishedProceduresRequestByPatient).ConfigureAwait(false);

        public async Task GenerateProcedureZip(GenerateProcedureZipRequest procedureZipRequest) =>
            await _managerServiceClient.GenerateProcedureZip(procedureZipRequest).ConfigureAwait(false);

        public async Task<GetPhysiciansSearchResponse> GetPhysicians(GetPhysiciansSearchRequest getPhysiciansSearchRequest) =>
            await _serviceSearchClient.GetPhysicians(getPhysiciansSearchRequest).ConfigureAwait(false);
        public async Task ShareProcedure(ShareProcedureRequest shareProcedureRequest) =>
            await _managerServiceClient.ShareProcedure(shareProcedureRequest).ConfigureAwait(false);

    }
}
