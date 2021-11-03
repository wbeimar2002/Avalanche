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
    }
}
