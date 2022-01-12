using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Google.Protobuf.WellKnownTypes;

using Ism.Common.Core.Configuration.Models;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.SystemState.Client;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Patients
{
    public class PatientsManager : IPatientsManager
    {
        private readonly IPieService _pieService;
        private readonly IDataManagementService _dataManagementService;
        private readonly IStateClient _stateClient;
        private readonly IActiveProcedureManager _activeProcedureManager;

        // TODO: remove this when we figure out how to clean up dependencies
        private readonly IRoutingManager _routingManager;

        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserModel user;
        private readonly ConfigurationContext _configurationContext;

        private readonly RecorderConfiguration _recorderConfiguration;
        private readonly SetupConfiguration _setupConfiguration;

        public PatientsManager(IPieService pieService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IDataManagementService dataManagementService,
            IStateClient stateClient,
            IActiveProcedureManager activeProcedureManager,
            IRoutingManager routingManager,
            IHttpContextAccessor httpContextAccessor,
            RecorderConfiguration recorderConfiguration,
            SetupConfiguration setupConfiguration
            )
        {
            _pieService = pieService;
            _accessInfoFactory = accessInfoFactory;
            _dataManagementService = dataManagementService;
            _mapper = mapper;
            _stateClient = stateClient;
            _activeProcedureManager = activeProcedureManager;
            _routingManager = routingManager;
            _httpContextAccessor = httpContextAccessor;
            _recorderConfiguration = recorderConfiguration;
            _setupConfiguration = setupConfiguration;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<PatientViewModel> RegisterPatient(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);

            ValidateDynamicConditions(newPatient);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            if (newPatient.Physician == null)
            {
                if (_setupConfiguration.Registration.Manual.AutoFillPhysician)
                {
                    newPatient.Physician = new PhysicianModel()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };
                }
            }

            await CheckProcedureType(newPatient.ProcedureType, newPatient.Department).ConfigureAwait(false);

            await AllocateNewProcedure(newPatient, false).ConfigureAwait(false);

            return newPatient;
        }

        private void ValidateDynamicConditions(PatientViewModel patient)
        {
            foreach (var item in _setupConfiguration.PatientInfo.Where(f => f.Required))
            {
                switch (item.Id)
                {
                    case "firstName":
                        Preconditions.ThrowIfNull(nameof(patient.FirstName), patient.FirstName);
                        break;
                    case "sex":
                        Preconditions.ThrowIfNull(nameof(patient.Sex), patient.Sex);
                        break;
                    case "dateOfBirth":
                        Preconditions.ThrowIfNull(nameof(patient.DateOfBirth), patient.DateOfBirth);
                        break;

                    case "physician":
                        Preconditions.ThrowIfNull(nameof(patient.Physician), patient.Physician);
                        break;
                    case "department":
                        Preconditions.ThrowIfNull(nameof(patient.Department), patient.Department);
                        break;
                    case "procedureType":
                        Preconditions.ThrowIfNull(nameof(patient.ProcedureType), patient.ProcedureType);
                        break;
                    //case "accessionNumber": TODO: Pending send the value from Register and Update
                        //    Preconditions.ThrowIfNull(nameof(patient.Accession), patient.Accession);
                        //    break;
                }
            }
        }

        public async Task<PatientViewModel> QuickPatientRegistration()
        {
            var quickRegistrationDateFormat = _setupConfiguration.Registration.Quick.DateFormat;
            var formattedDate = DateTime.UtcNow.ToLocalTime().ToString(quickRegistrationDateFormat);

            //TODO: Pending check this default data
            var newPatient = new PatientViewModel()
            {
                MRN = $"{formattedDate}MRN",
                DateOfBirth = DateTime.UtcNow.ToLocalTime(),
                FirstName = $"{formattedDate}FirstName",
                LastName = $"{formattedDate}LastName",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "U"
                },
                Physician = new PhysicianModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            await AllocateNewProcedure(newPatient, true).ConfigureAwait(false);

            return newPatient;
        }

        public async Task UpdatePatient(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);

            ValidateDynamicConditions(existingPatient);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            await CheckProcedureType(existingPatient.ProcedureType, existingPatient.Department).ConfigureAwait(false);

            var request = _mapper.Map<PatientViewModel, UpdatePatientRecordRequest>(existingPatient);
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await AllocateNewProcedure(existingPatient, false).ConfigureAwait(false);

            await _pieService.UpdatePatient(request).ConfigureAwait(false);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _pieService.DeletePatient(new DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = id
            }).ConfigureAwait(false);
        }

        public async Task<IList<PatientViewModel>> Search(PatientKeywordSearchFilterViewModel filter)
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

            var request = _mapper.Map<SearchRequest>(filter);
            request.SearchCultureName = cultureName;
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            var queryResult = await _pieService.Search(request).ConfigureAwait(false);

            return _mapper.Map<IList<PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);
        }

        public async Task<IList<PatientViewModel>> Search(PatientDetailsSearchFilterViewModel filter)
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
                Department = filter.Department,
                ProcedureId = filter.ProcedureId,
            };

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var request = _mapper.Map<SearchRequest>(filter);
            request.SearchCultureName = cultureName;
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            var queryResult = await _pieService.Search(request).ConfigureAwait(false);

            return _mapper.Map<IList<PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);

        }

        private async Task CheckProcedureType(ProcedureTypeModel procedureType, DepartmentModel department)
        {
            //incase user is not selected or entered a procedure type, assign it to Unknown like in QuickRegister
            if (string.IsNullOrEmpty(procedureType.Name) || procedureType.Name.Length == 0)
            {
                procedureType.Id = 0;
                procedureType.Name = "Unknown";
            }
            else
            {
                //TODO: Validate department support
                var existingProcedureType = await _dataManagementService.GetProcedureType(new Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest()
                {
                    ProcedureTypeName = procedureType.Name,
                    DepartmentId = Convert.ToInt32(department.Id),
                }).ConfigureAwait(false);

                if (existingProcedureType.Id == 0 && string.IsNullOrEmpty(existingProcedureType.Name))
                {
                    await _dataManagementService.AddProcedureType(new Ism.Storage.DataManagement.Client.V1.Protos.AddProcedureTypeRequest()
                    {
                        ProcedureTypeName = procedureType.Name,
                        DepartmentId = Convert.ToInt32(department.Id),
                    }).ConfigureAwait(false);
                }
            }
        }

        private async Task PublishActiveProcedure(PatientViewModel patient, ProcedureAllocationViewModel procedure)
        {
            if (patient != null && procedure != null)
            {
                await _stateClient.PersistData(new Ism.SystemState.Models.Procedure.ActiveProcedureState(
                    patient: _mapper.Map<Ism.SystemState.Models.Procedure.Patient>(patient),
                    images: new List<Ism.SystemState.Models.Procedure.ProcedureImage>(),
                    videos: new List<Ism.SystemState.Models.Procedure.ProcedureVideo>(),
                    backgroundVideos: new List<Ism.SystemState.Models.Procedure.ProcedureVideo>(),
                    libraryId: procedure.ProcedureId.Id,
                    repositoryId: procedure.ProcedureId.RepositoryName,

                    procedureRelativePath: procedure.RelativePath,
                    department: _mapper.Map<Ism.SystemState.Models.Procedure.Department>(patient.Department),
                    procedureType: _mapper.Map<Ism.SystemState.Models.Procedure.ProcedureType>(patient.ProcedureType),
                    physician: _mapper.Map<Ism.SystemState.Models.Procedure.Physician>(patient.Physician),
                    requiresUserConfirmation: false,

                    // TODO:
                    procedureStartTimeUtc: DateTimeOffset.UtcNow,

                    // TODO:
                    procedureTimezoneId: TimeZoneInfo.Local.Id,
                    isClinical: true,
                    notes: new List<Ism.SystemState.Models.Procedure.ProcedureNote>()
,
                    accession: null,
                    recordingEvents: new List<Ism.SystemState.Models.Procedure.VideoRecordingEvent>(),
                    recordingMode: _mapper.Map<Ism.SystemState.Models.Procedure.BackgroundRecordingMode>(patient.BackgroundRecordingMode))).ConfigureAwait(false);
            }
            else
            {
                await _stateClient.PersistData<Ism.SystemState.Models.Procedure.ActiveProcedureState>(null).ConfigureAwait(false);
            }

            // TODO: figure out how to do dependencies better
            // maybe have a separate data/event for when a patient is registered
            // routing manager subscribes to that event and we can have a cleaner dependency graph
            await (_routingManager?.PublishDefaultDisplayRecordingState()).ConfigureAwait(false);
        }

        private async Task AllocateNewProcedure(PatientViewModel patient, bool useconfiguredBackgroundRecordingMode)
        {
            var allocatedProcedure = await _activeProcedureManager.AllocateNewProcedure().ConfigureAwait(false);

            if (useconfiguredBackgroundRecordingMode)
            {
                var configuredBackgroundRecordingMode = _recorderConfiguration.BackgroundRecordingMode;

                //AlwaysStartOnCapture: This value can be configured in maintenance but it is not used in the Media system to control the behavior
                if (configuredBackgroundRecordingMode == Shared.Infrastructure.Enumerations.BackgroundRecordingMode.AlwaysStartOnCapture)
                {
                    patient.BackgroundRecordingMode = Shared.Infrastructure.Enumerations.BackgroundRecordingMode.StartOnMediaCapture;
                }
                else
                {
                    patient.BackgroundRecordingMode = configuredBackgroundRecordingMode;
                }
            }

            await PublishActiveProcedure(patient, allocatedProcedure).ConfigureAwait(false);
        }
    }
}
