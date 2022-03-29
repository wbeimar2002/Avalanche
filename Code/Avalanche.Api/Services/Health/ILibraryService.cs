using System.Threading.Tasks;
using Ism.Library.V1.Protos;

namespace Avalanche.Api.Services.Health
{
    public interface ILibraryService
    {
        Task DeleteActiveProcedureMediaItem(DeleteActiveProcedureMediaItemRequest deleteActiveProcedureMediaItemRequest);
        Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest);
        Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest);
        Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest);
        Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest);
        Task UpdateProcedure(UpdateProcedureRequest updateProcedureRequest);
        Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest);
        Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest);
        Task<GetFinishedProceduresResponse> GetFinishedProceduresByPatient(GetFinishedProceduresRequestByPatient getFinishedProceduresRequestByPatient);
        Task DownloadProcedure(ProcedureDownloadRequest procedureDownloadRequest);
        Task<GetPhysiciansSearchResponse> GetPhysicians(GetPhysiciansSearchRequest getPhysiciansSearchRequest);
        Task ShareProcedure(ShareProcedureRequest shareProcedureRequest);
        Task<ReindexRepositoryResponse> ReindexRepository(ReindexRepositoryRequest reindexRepositoryRequest);
    }
}
