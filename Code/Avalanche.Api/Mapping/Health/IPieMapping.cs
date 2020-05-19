using Avalanche.Api.ViewModels;
using Ism.PatientInfoEngine.Common.Core.Models;
using Ism.Storage.Common.Core.PatientList.Models;

namespace Avalanche.Api.Mapping.Health
{
    public interface IPieMapping
    {
        PatientSearchFields GetDetailsSearchFields(PatientDetailsSearchFilterViewModel filter);

        Shared.Domain.Models.Patient ToApiPatient(PatientRecord pieRecord);
    }
}
