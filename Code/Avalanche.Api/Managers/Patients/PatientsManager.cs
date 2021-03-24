using AutoMapper;

using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;

using Google.Protobuf.WellKnownTypes;

using Ism.Common.Core.Configuration.Models;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.SystemState.Client;

using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Patients
{
    public class PatientsManager : IPatientsManager
    {
        readonly IPieService _pieService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly IStorageService _storageService;
        readonly IMapper _mapper;
        readonly IDataManagementService _dataManagementService;
        readonly IStateClient _stateClient;
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly IProceduresManager _proceduresManager;

        readonly UserModel user;
        readonly ConfigurationContext configurationContext;

        public PatientsManager(IPieService pieService, 
            IAccessInfoFactory accessInfoFactory,
            IStorageService storageService,
            IMapper mapper, 
            IDataManagementService dataManagementService,
            IStateClient stateClient,
            IProceduresManager proceduresManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _pieService = pieService;
            _storageService = storageService;
            _accessInfoFactory = accessInfoFactory;
            _dataManagementService = dataManagementService;
            _mapper = mapper;
            _stateClient = stateClient;
            _proceduresManager = proceduresManager;
            _httpContextAccessor = httpContextAccessor;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<PatientViewModel> RegisterPatient(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.DateOfBirth), newPatient.DateOfBirth);
            Preconditions.ThrowIfNull(nameof(newPatient.FirstName), newPatient.FirstName);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex), newPatient.Sex);
            Preconditions.ThrowIfNull(nameof(newPatient.Sex.Id), newPatient.Sex.Id);
            Preconditions.ThrowIfNull(nameof(newPatient.ProcedureType.Name), newPatient.ProcedureType.Name);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            newPatient.AccessInformation = _mapper.Map<AccessInfoModel>(accessInfo);

            var setupSettings = await _storageService.GetJsonDynamic("SetupSettingsData", 1, configurationContext);

            //TODO: Pending facility
            if (newPatient.Physician == null)
            {
                if (setupSettings.Registration.Manual.AutoFillPhysician)
                {
                    newPatient.Physician = new PhysicianModel()
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

            await CheckProcedureType(newPatient.ProcedureType, newPatient.Department);

            var allocatedProcedure = await _proceduresManager.AllocateNewProcedure();

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>(newPatient);
            var result = await _pieService.RegisterPatient(patientRequest);
            PublishActiveProcedure(newPatient, allocatedProcedure);

            var response = _mapper.Map<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse, PatientViewModel>(result);
            return response;
        }

        public async Task<PatientViewModel> QuickPatientRegistration()
        {
            var setupSettings = await _storageService.GetJsonDynamic("SetupSettingsData", 1, configurationContext);
            string quickRegistrationDateFormat = setupSettings.Registration.Quick.DateFormat;
            string formattedDate = DateTime.UtcNow.ToLocalTime().ToString(quickRegistrationDateFormat);

            //TODO: Pending facility
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
                Department = new DepartmentModel()
                {
                    Id = 0,
                    Name = "Unknown"
                },
                ProcedureType = new ProcedureTypeModel() //TODO: What should be this value
                {
                    Id = 0,
                    Name = "Unknown"
                },
                //TODO: Performing physician is administrator by default
                //Which are theose values? Temporary I am assigning the user that made the request
                Physician = new PhysicianModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            newPatient.AccessInformation = _mapper.Map<AccessInfoModel>(accessInfo);

            var allocatedProcedure = await _proceduresManager.AllocateNewProcedure();

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>(newPatient);
            var result = await _pieService.RegisterPatient(patientRequest);
            PublishActiveProcedure(newPatient, allocatedProcedure);

            return _mapper.Map<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse, PatientViewModel>(result);
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
            Preconditions.ThrowIfNull(nameof(existingPatient.ProcedureType.Name), existingPatient.ProcedureType.Name);

            var setupSettings = await _storageService.GetJsonDynamic("SetupSettingsData", 1, configurationContext);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            existingPatient.AccessInformation = _mapper.Map<AccessInfoModel>(accessInfo);

            await CheckProcedureType(existingPatient.ProcedureType, existingPatient.Department);

            var patientRequest = _mapper.Map<PatientViewModel, Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest>(existingPatient);
            await _pieService.UpdatePatient(patientRequest);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.PatientList.Client.V1.Protos.AccessInfoMessage>(accessInfo);

            await _pieService.DeletePatient(new Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = id
            });
        }

        public async Task<IList<PatientViewModel>> Search(PatientKeywordSearchFilterViewModel filter)
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
            filter.AccessInformation = _mapper.Map<AccessInfoModel>(accessInfo);

            var request = _mapper.Map<Ism.PatientInfoEngine.V1.Protos.SearchRequest>(filter);
            var queryResult = await _pieService.Search(request);

            return _mapper.Map<IList<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);
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

            //TODO: Facility is internal??? just backend??? Un check box en configuraciones que dice si se filtra o no por facility
            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            filter.AccessInformation = _mapper.Map<AccessInfoModel>(accessInfo);

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            filter.CultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var request = _mapper.Map<Ism.PatientInfoEngine.V1.Protos.SearchRequest>(filter);
            var queryResult = await _pieService.Search(request);

            return _mapper.Map<IList<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);

        }

        private async Task CheckProcedureType(ProcedureTypeModel procedureType, DepartmentModel department)
        {
            //TODO: Validate department support
            var existingProcedureType = await _dataManagementService.GetProcedureType(new GetProcedureTypeRequest()
            {
                ProcedureTypeId = Convert.ToInt32(procedureType.Id),
                DepartmentId = Convert.ToInt32(department.Id),
            });

            if (existingProcedureType.Id == 0  && string.IsNullOrEmpty(existingProcedureType.Name))
            {
                await _dataManagementService.AddProcedureType(new AddProcedureTypeRequest()
                {
                    ProcedureType = new ProcedureTypeMessage()
                    {
                        Id = Convert.ToInt32(procedureType.Id),
                        Name = procedureType.Name,
                        DepartmentId = Convert.ToInt32(department.Id),
                    }
                });
            }
        }

        private void PublishActiveProcedure(PatientViewModel patient, ProcedureAllocationViewModel allocatedProcedure)
        {
            if (null != patient)
            {
                _stateClient.PersistData(new Ism.SystemState.Models.Procedure.ActiveProcedureState(
                    patient: _mapper.Map<Ism.SystemState.Models.Procedure.Patient>(patient),
                    images: new List<Ism.SystemState.Models.Procedure.ProcedureImage>(),
                    videos: new List<Ism.SystemState.Models.Procedure.ProcedureVideo>(),
                    libraryId: allocatedProcedure.ProcedureId.Id,
                    repositoryId: allocatedProcedure.ProcedureId.RepositoryName,

                    procedureRelativePath: allocatedProcedure.RelativePath,

                    department: _mapper.Map<Ism.SystemState.Models.Procedure.Department>(patient.Department),
                    procedureType: _mapper.Map<Ism.SystemState.Models.Procedure.ProcedureType>(patient.ProcedureType),
                    physician: _mapper.Map<Ism.SystemState.Models.Procedure.Physician>(patient.Physician),
                    requiresUserConfirmation: false,

                    // TODO:
                    procedureStartTimeUtc: DateTimeOffset.UtcNow,

                    // TODO:
                    procedureTimezoneId: TimeZoneInfo.Local.Id));
            }
            else
            {
                _stateClient.PersistData<Ism.SystemState.Models.Procedure.ActiveProcedureState>(null); 
            }
        }
    }
}
