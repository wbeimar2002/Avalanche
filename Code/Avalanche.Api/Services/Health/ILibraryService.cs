using Ism.Library.V1.Protos;

using System.Threading.Tasks;

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
        Task<ReindexRepositoryResponse> ReindexRepository(string repositoryName);
        Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest);
        Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest);
        Task<GetFinishedProceduresResponse> GetFinishedProceduresByPatient(GetFinishedProceduresRequestByPatient getFinishedProceduresRequestByPatient);
        Task GenerateProcedureZip(GenerateProcedureZipRequest procedureZipRequest);
        Task<GetPhysiciansSearchResponse> GetPhysicians(GetPhysiciansSearchRequest getPhysiciansSearchRequest);
    }
}
