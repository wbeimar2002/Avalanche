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

        Task ConfirmActiveProcedure();
        Task DeleteActiveProcedureMedia(ProcedureContentType procedureContentType, Guid contentId);
        Task DiscardActiveProcedure();
        Task FinishActiveProcedure();

        Task<ProcedureAllocationViewModel> AllocateNewProcedure();

        Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter);
        Task<ProcedureViewModel> GetProcedureDetails(string id);

    }
}