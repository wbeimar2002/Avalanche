using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public class PatientsManager : IPatientsManager
    {
        readonly IPieService _pieService;

        public PatientsManager(IPieService pieService)
        {
            _pieService = pieService;
        }

        public Task<Patient> DeletePatient(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Physician>> GetPhysiciansByPatient(string patiendId)
        {
            return await _pieService.GetPhysiciansByPatient(patiendId);
        }

        public async Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId)
        {
            return await _pieService.GetProceduresByPhysicianAndPatient(patiendId, physicianId);
        }

        public async Task<Patient> RegisterPatient(Patient newPatient)
        {
            return await _pieService.RegisterPatient(newPatient);
        }

        public async Task<Patient> RegisterQuickPatient()
        {
            return await _pieService.RegisterQuickPatient();
        }

        public async Task<List<Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            return await _pieService.Search(filter);
        }

        public async Task<List<Patient>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            return await _pieService.Search(filter);
        }

        public Task<Patient> UpdatePatient(Patient existing)
        {
            throw new NotImplementedException();
        }
    }
}
