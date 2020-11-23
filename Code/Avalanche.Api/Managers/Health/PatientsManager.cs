using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Ism.PatientInfoEngine.V1.Protos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public class PatientsManager : IPatientsManager
    {
        readonly IPieService _pieService;
        readonly ISettingsService _settingsService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly IMapper _mapper;
        readonly IDataManagementService _dataManagementService;

        public PatientsManager(IPieService pieService, 
            ISettingsService settingsService, 
            IAccessInfoFactory accessInfoFactory, 
            IMapper mapper, 
            IDataManagementService dataManagementService)
        {
            _pieService = pieService;
            _settingsService = settingsService;
            _accessInfoFactory = accessInfoFactory;
            _dataManagementService = dataManagementService;
            _mapper = mapper;
        }

        public async Task<Shared.Domain.Models.Patient> RegisterPatient(PatientViewModel newPatient, Avalanche.Shared.Domain.Models.User user)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.DateOfBirth), newPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(newPatient.FirstName), newPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex), newPatient.Sex);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex.Id), newPatient.Sex.Id);
            Preconditions.ThrowIfNull(nameof(newPatient.ProcedureType.Value), newPatient.ProcedureType.Value);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            newPatient.AccessInformation = _mapper.Map<AccessInfo>(accessInfo);

            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);

            //TODO: Pending facility
            //TODO: Configurable in maintenance (on/off) - user logged in is auto-filled as physician when doing manual registration
            //TODO: What about the procedure type and department. How this should be filled?
            //TODO: What about facility
            if (newPatient.Physician == null)
            {
                if (setupSettings.AutoFillPhysician)
                {
                    newPatient.Physician = new Physician()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };
                }
                else
                {
                    newPatient.Physician = newPatient.Physician;
                }
            }

            await CheckProcedureType(newPatient.ProcedureType.Value, newPatient.Department?.Value, setupSettings);

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>(newPatient);
            var result = await _pieService.RegisterPatient(patientRequest);

            var response = _mapper.Map<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse, Shared.Domain.Models.Patient>(result);
            return response;
        }

        public async Task<Shared.Domain.Models.Patient> QuickPatientRegistration(Avalanche.Shared.Domain.Models.User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);
            string quickRegistrationDateFormat = setupSettings.QuickRegistrationDateFormat;
            string formattedDate = DateTime.UtcNow.ToLocalTime().ToString(quickRegistrationDateFormat);

            //TODO: Pending facility
            //TODO: Pending check this default data
            var newPatient = new PatientViewModel()
            {
                ScopeSerialNumber = "???", //TODO: How this is related to
                MRN = $"{formattedDate}MRN",
                DateOfBirth = DateTime.UtcNow.ToLocalTime(),
                FirstName = $"{formattedDate}FirstName",
                LastName = $"{formattedDate}LastName",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "U"
                },
                Department = new KeyValuePairViewModel()
                {
                    Id = "Unknown",
                    Value = "Unknown"
                },
                ProcedureType = new KeyValuePairViewModel() //TODO: What should be this value
                {
                    Id = "Unknown",
                    Value = "Unknown"
                },
                //TODO: Performing physician is administrator by default
                //Which are the values?
                Physician = new Physician()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            newPatient.AccessInformation = _mapper.Map<AccessInfo>(accessInfo);

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>(newPatient);
            var result = await _pieService.RegisterPatient(patientRequest);

            return _mapper.Map<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse, Shared.Domain.Models.Patient>(result);
        }

        public async Task UpdatePatient(PatientViewModel existingPatient, Avalanche.Shared.Domain.Models.User user)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.DateOfBirth), existingPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(existingPatient.FirstName), existingPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);
            Preconditions.ThrowIfNull(nameof(existingPatient.Sex), existingPatient.Sex);
            Preconditions.ThrowIfNull(nameof(existingPatient.Sex.Id), existingPatient.Sex.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.ProcedureType.Value), existingPatient.ProcedureType.Value);

            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            existingPatient.AccessInformation = _mapper.Map<AccessInfo>(accessInfo);

            await CheckProcedureType(existingPatient.ProcedureType.Value, existingPatient.Department?.Value, setupSettings);

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>(existingPatient);
            await _pieService.UpdatePatient(patientRequest);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Core.PatientList.V1.Protos.AccessInfoMessage>(accessInfo);

            await _pieService.DeletePatient(new Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = id
            });
        }

        public async Task<IList<Shared.Domain.Models.Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfNull(nameof(filter.Term), filter.Term);

            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            //TODO: Facility is internal??? just backend??? Un check box en configuraciones que dice si se filtra o no por facility
            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            filter.CultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            filter.AccessInformation = _mapper.Map<AccessInfo>(accessInfo);

            var request = _mapper.Map<Ism.PatientInfoEngine.V1.Protos.SearchRequest>(filter);
            var queryResult = await _pieService.Search(request);

            return _mapper.Map<IList<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>, IList<Shared.Domain.Models.Patient>>(queryResult.UpdatedPatList);
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

            //TODO: Facility is internal??? just backend??? Un check box en configuraciones que dice si se filtra o no por facility
            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            filter.AccessInformation = _mapper.Map<AccessInfo>(accessInfo);

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            filter.CultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var request = _mapper.Map<Ism.PatientInfoEngine.V1.Protos.SearchRequest>(filter);
            var queryResult = await _pieService.Search(request);

            return _mapper.Map<IList<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>, IList<Shared.Domain.Models.Patient>>(queryResult.UpdatedPatList);

        }

        private async Task CheckProcedureType(string procedureTypeName, string departmentName, SetupSettings setupSettings)
        {
            var newProcedureType = new Ism.Storage.Core.DataManagement.V1.Protos.ProcedureTypeMessage()
            {
                Name = procedureTypeName,
                Department = departmentName
            };

            var existingProcedureType = await _dataManagementService.GetProcedureType(newProcedureType);

            if (string.IsNullOrEmpty(existingProcedureType.Name))
            {
                await _dataManagementService.AddProcedureType(new Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest()
                {
                    ProcedureType = newProcedureType,
                });
            }
        }
    }
}
