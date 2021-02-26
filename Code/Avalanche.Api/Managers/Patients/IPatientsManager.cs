using Avalanche.Api.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Patients
{
    public interface IPatientsManager
    {
        Task<IList<PatientViewModel>> Search(PatientKeywordSearchFilterViewModel filter);
        Task<IList<PatientViewModel>> Search(PatientDetailsSearchFilterViewModel filter);
        Task<PatientViewModel> RegisterPatient(PatientViewModel newPatient);
        Task<PatientViewModel> QuickPatientRegistration();
        Task UpdatePatient(PatientViewModel existing);
        Task DeletePatient(ulong id);
    }
}
