using Ism.Library.V1.Protos;

using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface ILibraryService
    {
        Task DeleteActiveProcedureMedia(DeleteActiveProcedureMediaRequest deleteActiveProcedureMediaRequest);
        Task DeleteActiveProcedureMediaItems(DeleteActiveProcedureMediaItemsRequest deleteActiveProcedureMediaItemsRequest);
        Task DiscardActiveProcedure(DiscardActiveProcedureRequest discardActiveProcedureRequest);
        Task CommitActiveProcedure(CommitActiveProcedureRequest commitActiveProcedureRequest);
        Task<AllocateNewProcedureResponse> AllocateNewProcedure(AllocateNewProcedureRequest allocateNewProcedureRequest);
        Task<ReindexRepositoryResponse> ReindexRepository(string repositoryName);
        Task<GetFinishedProcedureResponse> GetFinishedProcedure(GetFinishedProcedureRequest getFinishedProcedureRequest);
        Task<GetFinishedProceduresResponse> GetFinishedProcedures(GetFinishedProceduresRequest getFinishedProceduresRequest);
    }
}
