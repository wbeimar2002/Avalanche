using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PatientInfoEngine.Common.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task<Shared.Domain.Models.Patient> RegisterPatient(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            return await _pieService.RegisterPatient(new Patient()
            {
                MRN = newPatient.MRN,
                DateOfBirth = newPatient.DateOfBirth,
                FirstName = newPatient.FirstName,
                LastName = newPatient.LastName,
                Gender = newPatient.Gender.Id,
            });
        }

        public async Task<Shared.Domain.Models.Patient> RegisterQuickPatient()
        {
            //TODO Generate fake info
            var newPatient = new Patient()
            { 
            };

            return await _pieService.RegisterPatient(newPatient);
        }

        public async Task UpdatePatient(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            await _pieService.UpdatePatient(new Patient()
            {
                Id = existingPatient.Id,
                MRN = existingPatient.MRN,
                DateOfBirth = existingPatient.DateOfBirth,
                FirstName = existingPatient.FirstName,
                LastName = existingPatient.LastName,
                Gender = existingPatient.Gender.Id,
            });
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);
            await _pieService.DeletePatient(id);
        }

        public async Task<List<Shared.Domain.Models.Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);

            //TODO: Validate this with Zac
            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName);
        }

        public async Task<List<Shared.Domain.Models.Patient>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            var search = new PatientSearchFieldsMessage();
            //TODO: Other fields

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName);
        }
    }
}
