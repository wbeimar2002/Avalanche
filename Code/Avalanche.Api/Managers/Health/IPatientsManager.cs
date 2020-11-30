using Avalanche.Api.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public interface IPatientsManager
    {
        Task<IList<PatientViewModel>> Search(PatientKeywordSearchFilterViewModel filter);
        Task<IList<PatientViewModel>> Search(PatientDetailsSearchFilterViewModel filter);
        Task<PatientViewModel> RegisterPatient(PatientViewModel newPatient, Avalanche.Shared.Domain.Models.User user);
        Task<PatientViewModel> QuickPatientRegistration(Avalanche.Shared.Domain.Models.User user);
        Task UpdatePatient(PatientViewModel existing, Avalanche.Shared.Domain.Models.User user);
        Task DeletePatient(ulong id);
    }
}
