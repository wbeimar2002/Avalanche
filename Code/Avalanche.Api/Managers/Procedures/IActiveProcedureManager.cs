using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Procedures
{
    public interface IActiveProcedureManager
    {
        Task AllocateNewProcedure(int registrationMode, PatientViewModel? patient);
        Task<ActiveProcedureViewModel> GetActiveProcedure();

        Task ConfirmActiveProcedure();
        Task DeleteActiveProcedureMediaItem(ProcedureContentType procedureContentType, Guid contentId);
        Task DeleteActiveProcedureMediaItems(ProcedureContentType procedureContentType, IEnumerable<Guid> contentIds);
        Task DiscardActiveProcedure();
        Task FinishActiveProcedure();

        Task ApplyLabelToActiveProcedure(ContentViewModel labelContent);
        Task ApplyLabelToLatestImages(string label);
    }
}
