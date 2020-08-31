using AutoFixture;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.Protos;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public class PatientsManager : IPatientsManager
    {
        readonly IPieService _pieService;
        readonly ISettingsService _settingsService;
        public PatientsManager(IPieService pieService, ISettingsService settingsService)
        {
            _pieService = pieService;
            _settingsService = settingsService;
        }

        public async Task<Shared.Domain.Models.Patient> RegisterPatient(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.DateOfBirth), newPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(newPatient.FirstName), newPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);
            Preconditions.ThrowIfNull(nameof(newPatient.Gender), newPatient.Gender);
            Preconditions.ThrowIfNull(nameof(newPatient.Gender.Id), newPatient.Gender.Id);

            return await _pieService.RegisterPatient(new Patient()
            {
                MRN = newPatient.MRN,
                DateOfBirth = newPatient.DateOfBirth,
                FirstName = newPatient.FirstName,
                LastName = newPatient.LastName,
                Gender = newPatient.Gender.Id,
            },
            new ProcedureType()
            { 
                Id = newPatient.ProcedureType.Id
            }, 
            newPatient.Physician);
        }

        public async Task<Shared.Domain.Models.Patient> QuickPatientRegistration(System.Security.Claims.ClaimsPrincipal user)
        {
            //TODO:Validate this
            //Performing physician is administrator by default
            //Configurable in maintenance (on/off) - user logged in is auto-filled as physician when doing manual registration
            //Patient last name and MRN is of the local date, room name and time in the following format:  YYYY_mm_DD_HH_SS_MS_RM

            //TODO: This format should come from a configuration setting?
            string formattedDate = DateTime.UtcNow.ToString("yyyy_MM_dd_T_HH_mm_ss_ff");

            return await _pieService.RegisterPatient(new Patient()
            {
                MRN = $"{formattedDate}MRN",
                DateOfBirth = DateTime.UtcNow,
                FirstName = $"{formattedDate}FirstName",
                LastName = $"{formattedDate}LastName",
                Gender = "U",
            },
            new ProcedureType()
            {
                Id = "Unknown",
            },
            new Physician()
            {
                Id = "Admin",
                FirstName = "Admin",
                LastName = "Admin",
            });
            //new Physician() 
            //{
            //    Id = user.FindFirst("Id")?.Value,
            //    FirstName = user.FindFirst("FirstName")?.Value,
            //    LastName = user.FindFirst("LastName")?.Value,
            //});
        }

        public async Task UpdatePatient(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.DateOfBirth), existingPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(existingPatient.FirstName), existingPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);
            Preconditions.ThrowIfNull(nameof(existingPatient.Gender), existingPatient.Gender);
            Preconditions.ThrowIfNull(nameof(existingPatient.Gender.Id), existingPatient.Gender.Id);

            await _pieService.UpdatePatient(new Patient()
            {
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
            Preconditions.ThrowIfNull(nameof(filter.Term), filter.Term);

            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName);
        }

        public async Task<List<Shared.Domain.Models.Patient>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);

            var search = new PatientSearchFieldsMessage()
            {
                RoomName = filter.RoomName,
                LastName = filter.LastName,
                MRN = filter.MRN,
                MinDate = filter.MinDate?.ToTimestamp(),
                MaxDate = filter.MaxDate?.ToTimestamp(),
                Accession = filter.AccessionNumber,
                Keyword = null,
                Department = filter.DepartmentName,
                ProcedureId = filter.ProcedureId,
            };

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            return await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName);
        }
    }
}
