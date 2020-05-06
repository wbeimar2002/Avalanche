using AutoFixture;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public class PieServiceMock : IPieService
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
            newPatient.Id = Guid.NewGuid().ToString();
            return Task.FromResult(newPatient);
        }

        public Task<Patient> RegisterQuickPatient()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.Create<Patient>());
        }

        public Task<List<Patient>> Search(PatientSearchFilterViewModel filter)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Patient>(10).ToList());
        }
    }
}
