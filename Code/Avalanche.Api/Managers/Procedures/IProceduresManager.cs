using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Procedures
{
    public interface IProceduresManager
    {
        Task<ActiveProcedureViewModel> GetActiveProcedure();
        Task<ProcedureDetailsViewModel> GetProcedureDetails(string id);

        Task ConfirmActiveProcedure();
        Task DeleteActiveProcedureMedia(ProcedureContentType procedureContentType, Guid contentId);
        Task DiscardActiveProcedure();
        Task FinishActiveProcedure();

        Task<ProcedureAllocationViewModel> AllocateNewProcedure();
        Task<IEnumerable<ProcedureViewModel>> Search(ProcedureSearchFilterViewModel filter);
    }
}