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
        readonly IAccessInfoFactory _accessInfoFactory;

        public PatientsManager(IPieService pieService, IAccessInfoFactory accessInfoFactory)
        {
            _pieService = pieService;
            _accessInfoFactory = accessInfoFactory;
        }

        public async Task<Shared.Domain.Models.Patient> RegisterPatient(Shared.Domain.Models.Patient newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            return await _pieService.RegisterPatient(newPatient, accessInfo);
        }

        public async Task<Shared.Domain.Models.Patient> RegisterQuickPatient()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            //TODO Generate fake info
            var newPatient = new Patient()
            { 
            };

            return await _pieService.RegisterPatient(newPatient, accessInfo);
        }

        public async Task UpdatePatient(Shared.Domain.Models.Patient existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            await _pieService.UpdatePatient(existingPatient, accessInfo);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            await _pieService.DeletePatient(id, accessInfo);
        }

        public async Task<List<Shared.Domain.Models.Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            //TODO: Validate this with Zac
            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfo);
        }

        public async Task<List<Shared.Domain.Models.Patient>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var search = new PatientSearchFieldsMessage();
            //TODO: Other fields

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfo);
        }
    }
}
