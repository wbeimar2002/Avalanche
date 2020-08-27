using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public interface IPatientsManager
    {
        Task<List<Patient>> Search(PatientKeywordSearchFilterViewModel filter);
        Task<List<Patient>> Search(PatientDetailsSearchFilterViewModel filter);
        Task<Patient> RegisterPatient(PatientViewModel newPatient);
        Task<Patient> QuickPatientRegistration(System.Security.Claims.ClaimsPrincipal user);
        Task UpdatePatient(PatientViewModel existing);
        Task DeletePatient(ulong id);
    }
}
