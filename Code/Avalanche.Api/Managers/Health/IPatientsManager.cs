using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public interface IPatientsManager
    {
        Task<List<Patient>> Search(PatientSearchFilterViewModel filter);
        Task<List<Physician>> GetPhysiciansByPatient(string patiendId);
        Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId);
        Task<Patient> RegisterPatient(Patient newPatient);
        Task<Patient> RegisterQuickPatient();
    }
}
