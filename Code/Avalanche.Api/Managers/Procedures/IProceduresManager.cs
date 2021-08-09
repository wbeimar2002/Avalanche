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
        Task DeleteActiveProcedureMediaItem(ProcedureContentType procedureContentType, Guid contentId);
        Task DeleteActiveProcedureMediaItems(ProcedureContentType procedureContentType, IEnumerable<Guid> contentIds);
        Task DiscardActiveProcedure();
        Task FinishActiveProcedure();

        Task<ProcedureAllocationViewModel> AllocateNewProcedure();

        Task<ProceduresContainerViewModel> BasicSearch(ProcedureSearchFilterViewModel filter);
        Task<ProceduresContainerViewModel> AdvancedSearch(ProcedureAdvancedSearchFilterViewModel filter);
        Task<ProcedureViewModel> GetProcedureDetails(string id);

        Task ApplyLabelToActiveProcedure(ContentViewModel labelContent);
        Task UpdateProcedure(ProcedureViewModel procedureViewModel);

        Task ApplyLabelToLatestImages(string label);
    }
}
