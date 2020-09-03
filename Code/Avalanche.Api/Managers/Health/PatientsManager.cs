using AutoFixture;
using AutoMapper;
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
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly IMapper _mapper;

        public PatientsManager(IPieService pieService, ISettingsService settingsService, IAccessInfoFactory accessInfoFactory, IMapper mapper)
        {
            _pieService = pieService;
            _settingsService = settingsService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
        }

        public async Task<Shared.Domain.Models.Patient> RegisterPatient(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.DateOfBirth), newPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(newPatient.FirstName), newPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex), newPatient.Sex);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex.Id), newPatient.Sex.Id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>(accessInfo);

            var patient = _mapper.Map<PatientViewModel, Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage>(newPatient);

            var result = await _pieService.RegisterPatient(patient, accessInfoMessage);
            return _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage, Shared.Domain.Models.Patient>(result);
        }

        public async Task<Shared.Domain.Models.Patient> QuickPatientRegistration(System.Security.Claims.ClaimsPrincipal user)
        {
            //TODO:Validate this
            //Performing physician is administrator by default
            //Configurable in maintenance (on/off) - user logged in is auto-filled as physician when doing manual registration
            //Patient last name and MRN is of the local date, room name and time in the following format:  YYYY_mm_DD_HH_SS_MS_RM

            //TODO: This format should come from a configuration setting?
            string formattedDate = DateTime.UtcNow.ToString("yyyy_MM_dd_T_HH_mm_ss_ff");

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>(accessInfo);

            var newPatient = new PatientViewModel()
            {
                ScopeSerialNumber = "???", //TODO: How this is related to
                MRN = $"{formattedDate}MRN",
                DateOfBirth = DateTime.UtcNow,
                FirstName = $"{formattedDate}FirstName",
                LastName = $"{formattedDate}LastName",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "U"
                },
                ProcedureType = new KeyValuePairViewModel()
                {
                    Id =  "Unknown"
                },
                Physician = new Physician()
                {
                    Id = "Admin",
                    FirstName = "Admin",
                    LastName = "Admin",
                    //Id = user.FindFirst("Id")?.Value,
                    //FirstName = user.FindFirst("FirstName")?.Value,
                    //LastName = user.FindFirst("LastName")?.Value,
                }
            };
            
            var patient = _mapper.Map<PatientViewModel, Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage>(newPatient);

            var result = await _pieService.RegisterPatient(patient, accessInfoMessage);
            return _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage, Shared.Domain.Models.Patient>(result);
        }

        public async Task UpdatePatient(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.DateOfBirth), existingPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(existingPatient.FirstName), existingPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);
            Preconditions.ThrowIfNull(nameof(existingPatient.Sex), existingPatient.Sex);
            Preconditions.ThrowIfNull(nameof(existingPatient.Sex.Id), existingPatient.Sex.Id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>(accessInfo);

            var patient = _mapper.Map<PatientViewModel, Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage>(existingPatient);

            await _pieService.UpdatePatient(patient, accessInfoMessage);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>(accessInfo);

            await _pieService.DeletePatient(id, accessInfoMessage);
        }

        public async Task<IList<Shared.Domain.Models.Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfNull(nameof(filter.Term), filter.Term);

            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage>(accessInfo);

            var queryResult = await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfoMessage);
            return _mapper.Map<IList<Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>, IList<Shared.Domain.Models.Patient>>(queryResult);
        }

        public async Task<IList<Shared.Domain.Models.Patient>> Search(PatientDetailsSearchFilterViewModel filter)
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

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage>(accessInfo);

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var queryResult = await _pieService.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfoMessage);

            return _mapper.Map<IList<Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>, IList<Shared.Domain.Models.Patient>>(queryResult);

        }
    }
}
