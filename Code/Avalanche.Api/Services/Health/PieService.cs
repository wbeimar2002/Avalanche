using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public class PieService : IPieService
    {
        public Task<List<Physician>> GetPhysiciansByPatient(string patiendId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId)
        {
            throw new NotImplementedException();
        }

        public Task<Patient> RegisterPatient(Patient newPatient)
        {
            throw new NotImplementedException();
        }

        public Task<Patient> RegisterQuickPatient()
        {
            throw new NotImplementedException();
        }

        public Task<List<Patient>> Search(PatientSearchFilterViewModel filter)
        {
            throw new NotImplementedException();
        }
    }
}
