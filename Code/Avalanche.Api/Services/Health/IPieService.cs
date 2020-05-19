using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        //TODO at this level don't use viewmodel, this is temporary just for mocking
        Task<List<Patient>> Search(PatientKeywordSearchFilterViewModel filter);
        Task<List<Patient>> Search(PatientDetailsSearchFilterViewModel filter);
        Task<List<Physician>> GetPhysiciansByPatient(string patiendId);
        Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId);
        Task<Patient> RegisterPatient(Patient newPatient);
        Task<Patient> RegisterQuickPatient();
    }
}
