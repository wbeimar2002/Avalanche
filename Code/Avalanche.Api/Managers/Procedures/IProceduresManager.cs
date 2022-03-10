using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers.Procedures
{
    public interface IProceduresManager
    {
        Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter);
        Task<ProceduresContainerViewModel> SearchByPatient(string patientId);
        Task<ProcedureViewModel> GetProcedureDetails(ProcedureIdViewModel procedureIdViewModel);
        Task UpdateProcedure(ProcedureViewModel procedureViewModel);

        Task DownloadProcedure(ProcedureDownloadRequestViewModel procedureDownloadRequestModel);
        Task ShareProcedure(string repository, string id, List<string> userNames);
    }
}
