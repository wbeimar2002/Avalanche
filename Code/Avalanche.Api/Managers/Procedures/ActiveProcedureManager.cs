using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Library.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Ism.Utility.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.AspNetCore.Http;
using Avalanche.Api.Services.Security;
using Google.Protobuf.WellKnownTypes;

namespace Avalanche.Api.Managers.Procedures
{
    public class ActiveProcedureManager : IActiveProcedureManager
    {
        private readonly IStateClient _stateClient;
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IRecorderService _recorderService;
        private readonly IPatientsManager _patientsManager;
        private readonly IDataManagementService _dataManagementService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IDataManager _dataManager;
        private readonly LabelsConfiguration _labelsConfig;
        private readonly SetupConfiguration _setupConfiguration;
        private readonly UserModel _user;

        // TODO: remove this when we figure out how to clean up dependencies
        private readonly IRoutingManager _routingManager;
        private readonly ISecurityService _securityService;
        private readonly IPieService _pieService;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ActiveProcedureManager(IStateClient stateClient,
            ILibraryService libraryService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IRecorderService recorderService,
            IDataManager dataManager,
            LabelsConfiguration labelsConfig,
            IPatientsManager patientsManager,
            IDataManagementService dataManagementService,
            IRoutingManager routingManager,
            SetupConfiguration setupConfiguration,
            IHttpContextAccessor httpContextAccessor,
            ISecurityService securityService,
            IPieService pieService)
        {
            _stateClient = stateClient;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _recorderService = recorderService;
            _dataManager = dataManager;
            _labelsConfig = labelsConfig;
            _patientsManager = patientsManager;
            _dataManagementService = dataManagementService;
            _routingManager = routingManager;
            _setupConfiguration = setupConfiguration;
            _httpContextAccessor = httpContextAccessor;
            _securityService = securityService;
            _pieService = pieService;
            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        public async Task<ActiveProcedureViewModel> GetActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            var result = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            if (result != null)
            {
                result.RecorderState = (int?)(await _recorderService.GetRecorderState().ConfigureAwait(false)).State;
            }

            return result;
        }

        /// <summary>
        /// Set ActiveProcedure's "RequiresUserConfirmation" flag to false.
        /// </summary>
        public async Task ConfirmActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            activeProcedure.RequiresUserConfirmation = false;
            await _stateClient.PersistData(activeProcedure).ConfigureAwait(false);
        }

        public async Task DeleteActiveProcedureMediaItem(ProcedureContentType procedureContentType, Guid contentId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            if (procedureContentType == ProcedureContentType.Video)
            {
                ThrowIfVideoCannotBeDeleted(activeProcedure, contentId);
            }

            var request = new DeleteActiveProcedureMediaItemRequest()
            {
                ContentId = contentId.ToString(),
                ContentType = _mapper.Map<ContentType>(procedureContentType),
                ProcedureId = _mapper.Map<ProcedureIdMessage>(activeProcedure),
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo)
            };

            await _libraryService.DeleteActiveProcedureMediaItem(request).ConfigureAwait(false);
        }

        public async Task DeleteActiveProcedureMediaItems(ProcedureContentType procedureContentType, IEnumerable<Guid> contentIds)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            if (procedureContentType == ProcedureContentType.Video)
            {
                foreach (var videoContentId in contentIds)
                {
                    ThrowIfVideoCannotBeDeleted(activeProcedure, videoContentId);
                }
            }

            var request = new DeleteActiveProcedureMediaItemsRequest()
            {
                ContentType = _mapper.Map<ContentType>(procedureContentType),
                ProcedureId = _mapper.Map<ProcedureIdMessage>(activeProcedure),
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo)
            };
            request.ContentIds.AddRange(contentIds.Select(x => x.ToString()));

            await _libraryService.DeleteActiveProcedureMediaItems(request).ConfigureAwait(false);
        }

        public async Task DiscardActiveProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            var request = _mapper.Map<ActiveProcedureState, DiscardActiveProcedureRequest>(activeProcedure);

            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _recorderService.FinishProcedure().ConfigureAwait(false);
            await _libraryService.DiscardActiveProcedure(request).ConfigureAwait(false);

            await _stateClient.PublishEvent(new ProcedureDiscardedEvent()).ConfigureAwait(false);
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            var request = _mapper.Map<ActiveProcedureState, CommitActiveProcedureRequest>(activeProcedure);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _recorderService.FinishProcedure().ConfigureAwait(false);

            var procedureFinished = new ProcedureFinishedEvent
            {
                PatientInfo = activeProcedure.Patient
            };
            await _stateClient.PublishEvent(procedureFinished).ConfigureAwait(false);

            await _libraryService.CommitActiveProcedure(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Creating new procedure, with optional PatientViewModel parameter.
        /// </summary>
        /// <param name="registrationMode"></param>
        /// <param name="patient"></param>
        /// <returns>ProcedureAllocationViewModel</returns>
        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure(PatientRegistrationMode registrationMode, PatientViewModel? patient = null)
        {
            Preconditions.ThrowIfNull(nameof(registrationMode), registrationMode);

            patient = await GetRegisterPatient(registrationMode, patient);

            if (registrationMode != PatientRegistrationMode.Quick)
            {
                await CheckProcedureType(patient.ProcedureType, patient.Department).ConfigureAwait(false);
            }

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var response = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo),
                Clinical = true
            }).ConfigureAwait(false);

            var procedure = _mapper.Map<ProcedureAllocationViewModel>(response);
            var patientListSource = await GetPatientListSource().ConfigureAwait(false);

            await PublishPersistData(patient, procedure, patientListSource, (int)registrationMode).ConfigureAwait(false);

            await _routingManager.PublishDefaultDisplayRecordingState().ConfigureAwait(false);

            return _mapper.Map<ProcedureAllocationViewModel>(response);
        }

        public async Task ApplyLabelToActiveProcedure(ContentViewModel labelContent)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(labelContent.Label), labelContent.Label);

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            // If adhoc labels allowed option enabled, add label to store
            if (_labelsConfig.AdHocLabelsAllowed)
            {
                var newLabel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id).ConfigureAwait(false);
                if (newLabel == null || newLabel?.Id == 0)
                {
                    await _dataManager.AddLabel(new LabelModel
                    {
                        Name = labelContent.Label,
                        ProcedureTypeId = activeProcedure.ProcedureType?.Id
                    }).ConfigureAwait(false);
                }
            }

            //check label exist in store before associating the label to active procedure
            var labelModel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id).ConfigureAwait(false);
            if (labelModel == null || labelModel?.Id == 0)
            {
                throw new ArgumentException($"{nameof(labelContent.Label)} '{labelContent.Label}' does not exist and cannot be added", labelContent.Label);
            }

            try
            {
                if (labelContent.ProcedureContentType == ProcedureContentType.Image)
                {
                    var imageToEdit = activeProcedure.Images.First(y => y.ImageId == labelContent.ContentId);
                    imageToEdit.Label = labelContent.Label;
                }
                else
                {
                    var videoToEdit = activeProcedure.Videos.First(y => y.VideoId == labelContent.ContentId);
                    videoToEdit.Label = labelContent.Label;
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"{labelContent.ProcedureContentType} with {nameof(labelContent.ContentId)} {labelContent.ContentId} does not exist in {nameof(ActiveProcedureState)}", ex);
            }

            await _stateClient.AddOrUpdateData(activeProcedure, x =>
            {
                if (labelContent.ProcedureContentType == ProcedureContentType.Image)
                {
                    x.Replace(data => data.Images, activeProcedure.Images);
                }
                else
                {
                    x.Replace(data => data.Videos, activeProcedure.Videos);
                }
            }).ConfigureAwait(false);
        }

        public async Task ApplyLabelToLatestImages(string label)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(label), label);

            //get active procedure state
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            if (activeProcedure == null)
            {
                throw new InvalidOperationException("Active procedure does not exist");
            }

            //retrieve correlationid from the latest image from the list of images in active procedure state
            var correlationId = activeProcedure?.Images.OrderByDescending(img => img.CaptureTimeUtc).FirstOrDefault()?.CorrelationId;

            //check latest image contains valid correlationid (valid Guid)
            if (correlationId == null || correlationId == Guid.Empty)
            {
                throw new InvalidOperationException("Active procedure does not contain images or latest image(s) does not have valid correlation id");
            }
            //get all the images with the above correlationid from active procedure state
            var listOfImagesWithCorrelationId = activeProcedure?.Images.Where(x => x.CorrelationId == correlationId);

            //update label field for each image
            listOfImagesWithCorrelationId.ToList().ForEach(x => x.Label = label);

            //update active procedure state with latest changes to the images collection
            _ = await _stateClient.AddOrUpdateData(activeProcedure, x => x.Replace(data => data.Images, activeProcedure.Images)).ConfigureAwait(false);
        }

        /// <summary>
        /// Update patient in the procedure
        /// </summary>
        /// <param name="patient"></param>
        public async Task UpdateActiveProcedure(PatientViewModel patient)
        {
            Preconditions.ThrowIfNull(nameof(patient), patient);

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            var procedureViewModel = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);
            procedureViewModel.Patient = patient;
            procedureViewModel.Patient.MRN = patient.MRN;
            procedureViewModel.Patient.ProcedureType = patient.ProcedureType;

            activeProcedure.Physician = _mapper.Map<Physician>(patient.Physician);
            activeProcedure.Department = _mapper.Map<Department>(patient.Department);
            activeProcedure.Patient = _mapper.Map<Patient>(patient);
            activeProcedure.ProcedureType = _mapper.Map<ProcedureTypeModel, ProcedureType>(patient.ProcedureType);

            await _stateClient.PersistData(activeProcedure).ConfigureAwait(false);
        }

        private void ThrowIfVideoCannotBeDeleted(ActiveProcedureState activeProcedure, Guid videoContent)
        {
            var video = activeProcedure.Videos.Single(v => v.VideoId == videoContent);
            if (!video.VideoStopTimeUtc.HasValue)
            {
                throw new InvalidOperationException("Cannot delete video that is currently recording");
            }
        }

        private async Task PublishPersistData(PatientViewModel patient, ProcedureAllocationViewModel procedure, int patientListSource, int registrationMode)
        {
            var activeProcedureState = new ActiveProcedureState()
            {
                Patient = _mapper.Map<Patient>(patient),
                Images = new List<ProcedureImage>(),
                Videos = new List<ProcedureVideo>(),
                BackgroundVideos = new List<ProcedureVideo>(),
                LibraryId = procedure.ProcedureId.Id,
                RepositoryId = procedure.ProcedureId.RepositoryName,
                ProcedureRelativePath = procedure.RelativePath,
                Department = _mapper.Map<Department>(patient.Department),
                ProcedureType = _mapper.Map<ProcedureType>(patient.ProcedureType),
                Physician = _mapper.Map<Physician>(patient.Physician),
                RequiresUserConfirmation = false,
                // TODO:
                ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                // TODO:
                ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                IsClinical = true,
                Notes = new List<ProcedureNote>(),
                Accession = null,
                RecordingEvents = new List<VideoRecordingEvent>(),
                BackgroundRecordingMode = _mapper.Map<Ism.SystemState.Models.Procedure.BackgroundRecordingMode>(patient.BackgroundRecordingMode),
                RegistrationMode = (RegistrationMode)registrationMode,
                PatientListSource = (PatientListSource)patientListSource
            };

            await _stateClient.PersistData(activeProcedureState).ConfigureAwait(false);
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

        private async Task<PatientViewModel> GetPatientForManual(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);

            ValidateDynamicConditions(newPatient);

            newPatient.Physician = await GetSelectedPhysician(PatientRegistrationMode.Manual).ConfigureAwait(false);

            return newPatient;
        }

        public async Task<PatientViewModel> GetPatientForQuickRegistration()
        {
            var quickRegistrationDateFormat = _setupConfiguration.Registration.Quick.DateFormat;
            var formattedDate = DateTime.UtcNow.ToLocalTime().ToString(quickRegistrationDateFormat);

            var physician = await GetSelectedPhysician(PatientRegistrationMode.Quick).ConfigureAwait(false);

            //TODO: Pending check this default data
            return new PatientViewModel()
            {
                MRN = $"{formattedDate}MRN",
                DateOfBirth = DateTime.UtcNow.ToLocalTime(),
                FirstName = $"{formattedDate}FirstName",
                LastName = $"{formattedDate}LastName",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "U"
                },
                Physician = physician
            };
        }

        public async Task<PatientViewModel> ValidatePatientForUpdateRegistration(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);

            ValidateDynamicConditions(existingPatient);

            existingPatient.Physician = new PhysicianModel()
            {
                Id = _user.Id,
                FirstName = _user.FirstName,
                LastName = _user.LastName
            };

            return existingPatient;
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

        private async Task<int> GetPatientListSource()
        {
            var getSource = await _pieService.GetPatientListSource(new Empty()).ConfigureAwait(false);
            return getSource.Source;
        }

        private async Task<PhysicianModel?> GetSelectedPhysician(PatientRegistrationMode registrationMode)
        {
            var isAutoFillPhysicianEnabled = _setupConfiguration.Registration.Manual.AutoFillPhysician;

            return registrationMode switch
            {
                PatientRegistrationMode.Manual => await (isAutoFillPhysicianEnabled ? GetPhysician("currentUser") : GetPhysician("blank")).ConfigureAwait(false),
                PatientRegistrationMode.Quick => await (isAutoFillPhysicianEnabled ? GetPhysician("currentUser") : GetPhysician("administrator")).ConfigureAwait(false),
                _ => null,
            };
        }

        private async Task<PhysicianModel?> GetPhysician(string physicianReturned)
        {
            var systemAdministrator = await _securityService.FindByUserName("Administrator").ConfigureAwait(false);

            return physicianReturned switch
            {
                "currentUser" => new PhysicianModel()
                {
                    Id = _user.Id,
                    FirstName = _user.FirstName,
                    LastName = _user.LastName
                },
                "administrator" => new PhysicianModel()
                {
                    Id = systemAdministrator.User.Id,
                    FirstName = systemAdministrator.User.FirstName,
                    LastName = systemAdministrator.User.LastName
                },
                "blank" => new PhysicianModel()
                {
                    Id = 0,
                    FirstName = string.Empty,
                    LastName = string.Empty
                },
                _ => null,
            };
        }

        private async Task<PatientViewModel> GetRegisterPatient(PatientRegistrationMode registrationMode, PatientViewModel? patient) => registrationMode switch
        {
            PatientRegistrationMode.Quick => await GetPatientForQuickRegistration().ConfigureAwait(false),
            PatientRegistrationMode.Manual => await GetPatientForManual(patient).ConfigureAwait(false),
            PatientRegistrationMode.Update => await ValidatePatientForUpdateRegistration(patient).ConfigureAwait(false),
            _ => new PatientViewModel(),
        };
    }
}
