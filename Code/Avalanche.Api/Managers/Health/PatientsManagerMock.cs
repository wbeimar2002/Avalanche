using AutoFixture;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public class PatientsManagerMock : IPatientsManager
    {
        public Task<List<Physician>> GetPhysiciansByPatient(string patiendId)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Physician>(10).ToList());
        }

        public Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Procedure>(10).ToList());
        }

        public Task<Patient> RegisterPatient(Patient newPatient)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.Create<Patient>());
        }

        public Task<Patient> RegisterQuickPatient()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.Create<Patient>());
        }

        public Task<PatientListViewModel> Search(PatientSearchFilterViewModel filter)
        {
            var fixture = new Fixture();
            var list = fixture.CreateMany<Patient>(10).ToList();

            return Task.FromResult(new PatientListViewModel()
            {
                PageIndex = 0,
                Patients = list
            });
        }
    }
}
