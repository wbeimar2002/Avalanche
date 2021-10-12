using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers.Procedures
{
    public interface IProceduresManager
    {
        Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter);
        Task<ProceduresContainerViewModel> SearchByPatient(string patientId);
        Task<ProcedureViewModel> GetProcedureDetails(string id);
        Task UpdateProcedure(ProcedureViewModel procedureViewModel);
    }
}
