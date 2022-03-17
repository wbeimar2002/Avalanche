using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Library.V1.Protos;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Procedures
{
    public class ActiveProcedureManager : IActiveProcedureManager
    {
        private readonly IStateClient _stateClient;
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IRecorderService _recorderService;

        // SMELL: Why are we using both a service and manager from this class?
        private readonly IDataManagementService _dataManagementService;
        private readonly IDataManager _dataManager;

        private readonly LabelsConfiguration _labelsConfig;
        private readonly SetupConfiguration _setupConfiguration;
        private readonly UserModel _user;
        private readonly IRoutingManager _routingManager;
        private readonly ISecurityService _securityService;
        private readonly IPieService _pieService;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;
        public const string QuickRegisterDefaultStringValue = "Quick Register";

        private readonly ImmutableDictionary<ProcedureInfoField, PropertyInfo?> _fieldToPropertyMap =
            new Dictionary<ProcedureInfoField, PropertyInfo?>
            {
                { ProcedureInfoField.accessionNumber, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.AccessionNumber)) },
                { ProcedureInfoField.clinicalNotes, null },
                { ProcedureInfoField.dateOfBirth, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.DateOfBirth))! },
                { ProcedureInfoField.department, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Department))! },
                { ProcedureInfoField.diagnosis, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Diagnosis))! },
                { ProcedureInfoField.firstName, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.FirstName))! },
                { ProcedureInfoField.lastName, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.LastName))! },
                { ProcedureInfoField.mrn, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.MRN))! },
                { ProcedureInfoField.physician, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Physician))! },
                { ProcedureInfoField.procedureType, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.ProcedureType))! },
                { ProcedureInfoField.scopeSerialNumber, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.ScopeSerialNumber)) },
                { ProcedureInfoField.sex, typeof(PatientViewModel).GetProperty(nameof(PatientViewModel.Sex))! },
                { ProcedureInfoField.undefined, null }
            }.ToImmutableDictionary();

        public ActiveProcedureManager(IStateClient stateClient,
            ILibraryService libraryService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IRecorderService recorderService,
            IDataManager dataManager,
            LabelsConfiguration labelsConfig,
            IDataManagementService dataManagementService,
            IRoutingManager routingManager,
            SetupConfiguration setupConfiguration,
            IHttpContextAccessor httpContextAccessor,
            ISecurityService securityService,
            IPieService pieService
            //, IClock clock
        )
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
            _dataManagementService = dataManagementService;
            _routingManager = routingManager;
            _setupConfiguration = setupConfiguration;
            _securityService = securityService;
            _pieService = pieService;
            _user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        public async Task<ActiveProcedureViewModel?> GetActiveProcedure()
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
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ConfirmActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(ConfirmActiveProcedure)} cannot confirm if {nameof(ActiveProcedureState)} is null");
            }

            activeProcedure.RequiresUserConfirmation = false;
            await _stateClient.PersistData(activeProcedure).ConfigureAwait(false);
        }

        public async Task DeleteActiveProcedureMediaItem(ProcedureContentType procedureContentType, Guid contentId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(DeleteActiveProcedureMediaItem)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

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
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(DeleteActiveProcedureMediaItems)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

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
                AccessInfo = _mapper.Map<AccessInfoMessage>(_accessInfoFactory.GenerateAccessInfo())
            };
            request.ContentIds.AddRange(contentIds.Select(x => x.ToString()));

            await _libraryService.DeleteActiveProcedureMediaItems(request).ConfigureAwait(false);
        }

        public async Task DiscardActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(DiscardActiveProcedure)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

            var request = _mapper.Map<ActiveProcedureState, DiscardActiveProcedureRequest>(activeProcedure);

            request.AccessInfo = _mapper.Map<AccessInfoMessage>(_accessInfoFactory.GenerateAccessInfo());

            await _recorderService.FinishProcedure().ConfigureAwait(false);
            await _libraryService.DiscardActiveProcedure(request).ConfigureAwait(false);

            await _stateClient.PublishEvent(new ProcedureDiscardedEvent()).ConfigureAwait(false);
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(DiscardActiveProcedure)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

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
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure(PatientRegistrationMode registrationMode, PatientViewModel? patient = null)
        {
            Preconditions.ThrowIfNull(nameof(registrationMode), registrationMode);
            if (registrationMode != PatientRegistrationMode.Quick)
            {
                Preconditions.ThrowIfNull(nameof(patient), patient);
                // Use null-forgiving because we just checked if patient was null
                ValidateRequiredFields(patient!);
            }

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure != null)
            {
                throw new InvalidOperationException($"{nameof(AllocateNewProcedure)} cannot proceed when another procedure is already active.");
            }

            patient = await GetPatientForRegistration(registrationMode, patient).ConfigureAwait(false);

            if (registrationMode != PatientRegistrationMode.Quick)
            {
                await CheckProcedureType(patient.ProcedureType, patient.Department).ConfigureAwait(false);
            }

            var newProcedureResponse = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(_accessInfoFactory.GenerateAccessInfo()),
                Clinical = true
            }).ConfigureAwait(false);

            var patientListSource = await GetPatientListSource().ConfigureAwait(false);

            await PublishPersistData(patient, newProcedureResponse, patientListSource, registrationMode).ConfigureAwait(false);

            await _routingManager.PublishDefaultDisplayRecordingState().ConfigureAwait(false);

            return _mapper.Map<ProcedureAllocationViewModel>(newProcedureResponse);
        }

        public async Task ApplyLabelToActiveProcedure(ContentViewModel labelContent)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(labelContent.Label), labelContent.Label);

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(ApplyLabelToActiveProcedure)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

            // If adhoc labels allowed option enabled, add label to store
            if (_labelsConfig.AdHocLabelsAllowed)
            {
                var newLabel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id).ConfigureAwait(false);
                if (newLabel is null || newLabel?.Id == 0)
                {
                    _ = await _dataManager.AddLabel(new LabelModel
                    {
                        Name = labelContent.Label,
                        ProcedureTypeId = activeProcedure.ProcedureType?.Id
                    }).ConfigureAwait(false);
                }
            }

            //check label exist in store before associating the label to active procedure
            var labelModel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id).ConfigureAwait(false);
            if (labelModel is null || labelModel?.Id == 0)
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

            _ = await _stateClient.AddOrUpdateData(activeProcedure, x =>
            {
                if (labelContent.ProcedureContentType == ProcedureContentType.Image)
                {
                    _ = x.Replace(data => data.Images, activeProcedure.Images);
                }
                else
                {
                    _ = x.Replace(data => data.Videos, activeProcedure.Videos);
                }
            }).ConfigureAwait(false);
        }

        public async Task ApplyLabelToLatestImages(string label)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(label), label);

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(ApplyLabelToLatestImages)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

            // Retrieve correlationid from the latest image from the list of images in active procedure state
            var correlationId = activeProcedure?.Images.OrderByDescending(img => img.CaptureTimeUtc).FirstOrDefault()?.CorrelationId;

            // Check latest image contains valid correlationid (valid Guid)
            if (correlationId is null || correlationId == Guid.Empty)
            {
                throw new InvalidOperationException("Active procedure does not contain images or latest image(s) does not have valid correlation id");
            }
            // Get all the images with the above correlationid from active procedure state
            var listOfImagesWithCorrelationId = activeProcedure?.Images.Where(x => x.CorrelationId == correlationId);

            // Update label field for each image
            listOfImagesWithCorrelationId.ToList().ForEach(x => x.Label = label);

            // Update active procedure state with latest changes to the images collection
            // FYI: Use null=forgiving here since we already checked that active procedure is not null.
            // TODO: Add locking so another thread couldn't have Finished/Discard procedure since we last checked
            _ = await _stateClient.AddOrUpdateData(activeProcedure!, x => x.Replace(data => data.Images, activeProcedure!.Images)).ConfigureAwait(false);
        }

        /// <summary>
        /// Update patient in the procedure
        /// </summary>
        /// <param name="patient"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task UpdateActiveProcedure(PatientViewModel patient)
        {
            Preconditions.ThrowIfNull(nameof(patient), patient);
            ValidateRequiredFields(patient);

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            if (activeProcedure is null)
            {
                throw new InvalidOperationException($"{nameof(UpdateActiveProcedure)} cannot be called if {nameof(ActiveProcedureState)} is null");
            }

            var procedureViewModel = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);
            procedureViewModel.Patient = patient;
            procedureViewModel.Patient.MRN = patient.MRN;
            procedureViewModel.Patient.ProcedureType = patient.ProcedureType;

            activeProcedure.Physician = _mapper.Map<Physician>(patient.Physician);
            activeProcedure.Department = _mapper.Map<Department>(patient.Department);
            activeProcedure.Patient = _mapper.Map<Patient>(patient);
            activeProcedure.ProcedureType = _mapper.Map<ProcedureTypeModel, ProcedureType>(patient.ProcedureType ?? new ProcedureTypeModel());

            await _stateClient.PersistData(activeProcedure).ConfigureAwait(false);
        }

        private static void ThrowIfVideoCannotBeDeleted(ActiveProcedureState activeProcedure, Guid videoContent)
        {
            var video = activeProcedure.Videos.Single(v => v.VideoId == videoContent);
            if (!video.VideoStopTimeUtc.HasValue)
            {
                throw new InvalidOperationException("Cannot delete video that is currently recording");
            }
        }

        private async Task PublishPersistData(PatientViewModel patient, AllocateNewProcedureResponse procedure, int patientListSource, PatientRegistrationMode registrationMode)
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
                ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                IsClinical = true,
                Notes = new List<ProcedureNote>(),
                Accession = patient.AccessionNumber,
                RecordingEvents = new List<VideoRecordingEvent>(),
                BackgroundRecordingMode = _mapper.Map<Ism.SystemState.Models.Procedure.BackgroundRecordingMode>(patient.BackgroundRecordingMode),
                RegistrationMode = _mapper.Map<RegistrationMode>(registrationMode),
                PatientListSource = (PatientListSource)patientListSource // SMELL: Why is this being passed as an int?  Unsafe!  What if enum is reordered?  This will break
                // TODO: Diagnosis and Scope Serial Number are not currently persisted to ActiveProcedureState
            };

            await _stateClient.PersistData(activeProcedureState).ConfigureAwait(false);
        }

        private async Task CheckProcedureType(ProcedureTypeModel? procedureType, DepartmentModel? department)
        {
            if (procedureType is null)
            {
                procedureType = new ProcedureTypeModel();
            }

            if (department is null)
            {
                department = new DepartmentModel();
            }

            // Incase user is not selected or entered a procedure type, assign it to Unknown like in QuickRegister
            if (string.IsNullOrEmpty(procedureType.Name) || procedureType.Name.Length == 0)
            {
                procedureType.Id = 0;
                procedureType.Name = "Unknown";
            }
            else
            {
                var existingProcedureType = await _dataManagementService.GetProcedureType(
                    new GetProcedureTypeRequest()
                    {
                        ProcedureTypeName = procedureType.Name,
                        DepartmentId = Convert.ToInt32(department.Id),
                    }
                ).ConfigureAwait(false);

                if (existingProcedureType.Id == 0 && string.IsNullOrEmpty(existingProcedureType.Name))
                {
                    _ = await _dataManagementService.AddProcedureType(
                        new AddProcedureTypeRequest()
                        {
                            ProcedureTypeName = procedureType.Name,
                            DepartmentId = Convert.ToInt32(department.Id),
                        }
                    ).ConfigureAwait(false);
                }
            }
        }

        private async Task<PatientViewModel> GetPatientForManual(PatientViewModel newPatient)
        {
            Preconditions.ThrowIfNull(nameof(newPatient), newPatient);
            Preconditions.ThrowIfNull(nameof(newPatient.MRN), newPatient.MRN);
            Preconditions.ThrowIfNull(nameof(newPatient.LastName), newPatient.LastName);

            ValidateConfigurableRequiredFields(newPatient);

            newPatient.Physician = await GetSelectedPhysician(PatientRegistrationMode.Manual, newPatient.Physician).ConfigureAwait(false);

            return newPatient;
        }

        private async Task<PatientViewModel> GetPatientForQuickRegistration()
        {
            var quickRegistrationDateFormat = _setupConfiguration.Registration.Quick.DateFormat;
            var formattedDate = DateTime.UtcNow.ToLocalTime().ToString(quickRegistrationDateFormat, CultureInfo.InvariantCulture);

            var physician = await GetSelectedPhysician(PatientRegistrationMode.Quick).ConfigureAwait(false);
            const string roomName = "RoomName";

            // SMELL: We should not be performing business logic with a view model...
            var patient = new PatientViewModel();
            foreach (var requiredFieldId in _setupConfiguration.PatientInfo.Where(f => f.Required).Select(x => x.Id))
            {
                if (!_fieldToPropertyMap.TryGetValue(requiredFieldId, out var property))
                {
                    throw new InvalidOperationException($"{nameof(ProcedureInfoField)} of {requiredFieldId} is not mapped to a property of {nameof(PatientViewModel)}");
                }

                if (property is null)
                {
                    // If property exists in the map but the value is null then there is no valid handling during patient registration, e.g. Clinical Notes, and can be ignored
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(patient, QuickRegisterDefaultStringValue);
                }

                if (property.PropertyType == typeof(DepartmentModel))
                {
                    property.SetValue(patient, new DepartmentModel { Id = 0, Name = QuickRegisterDefaultStringValue });
                }

                if (property.PropertyType == typeof(ProcedureTypeModel))
                {
                    property.SetValue(patient, new ProcedureTypeModel { Id = 0, Name = QuickRegisterDefaultStringValue });
                }
            }

            // Change the MRN, DOB, FirstName and LastName with specific values
            var template = $"{formattedDate}_{roomName}";
            patient.MRN = template;
            patient.FirstName = template;
            patient.LastName = template;

            // AC says that for DOB "the intent is to just use an obviously bogus date."
            patient.DateOfBirth = DateTime.MaxValue;
            patient.Sex = new KeyValuePairViewModel()
            {
                Id = _setupConfiguration.Registration.Quick.DefaultSex.ToString()
            };
            patient.Physician = physician;
            return patient;
        }

        private void ValidateRequiredFields(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);

            ValidateConfigurableRequiredFields(existingPatient);
        }

        private void ValidateConfigurableRequiredFields(PatientViewModel patient)
        {
            foreach (var item in _setupConfiguration.PatientInfo.Where(f => f.Required))
            {
                switch (item.Id)
                {
                    case ProcedureInfoField.firstName:
                        Preconditions.ThrowIfNull(nameof(patient.FirstName), patient.FirstName);
                        break;
                    case ProcedureInfoField.sex:
                        Preconditions.ThrowIfNull(nameof(patient.Sex), patient.Sex);
                        break;
                    case ProcedureInfoField.dateOfBirth:
                        Preconditions.ThrowIfNull(nameof(patient.DateOfBirth), patient.DateOfBirth);
                        break;
                    case ProcedureInfoField.physician:
                        Preconditions.ThrowIfNull(nameof(patient.Physician), patient.Physician);
                        break;
                    case ProcedureInfoField.department:
                        Preconditions.ThrowIfNull(nameof(patient.Department), patient.Department);
                        break;
                    case ProcedureInfoField.procedureType:
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
            var getSource = await _pieService.GetPatientListSource().ConfigureAwait(false);
            return getSource.Source;
        }

        private async Task<PhysicianModel?> GetSelectedPhysician(PatientRegistrationMode registrationMode, PhysicianModel userSelectedPhysician = null)
        {
            var isAutoFillPhysicianEnabled = _setupConfiguration.Registration.Manual.PhysicianAsLoggedInUser;

            return registrationMode switch
            {
                PatientRegistrationMode.Manual => isAutoFillPhysicianEnabled ? GetCurrentUserAsPhysician() : userSelectedPhysician,
                PatientRegistrationMode.Quick => isAutoFillPhysicianEnabled ? GetCurrentUserAsPhysician() : await GetAdminAsPhysician().ConfigureAwait(false),
                _ => null,
            };

            PhysicianModel GetCurrentUserAsPhysician() =>
                new PhysicianModel()
                {
                    Id = _user.Id,
                    FirstName = _user.FirstName,
                    LastName = _user.LastName
                };

            async Task<PhysicianModel> GetAdminAsPhysician()
            {
                var defaultUserName = _setupConfiguration.Registration.Quick.DefaultUserName ?? "Administrator";
                var systemAdministrator = await _securityService.GetUser(defaultUserName).ConfigureAwait(false);
                return new PhysicianModel()
                {
                    Id = systemAdministrator.User.Id,
                    FirstName = systemAdministrator.User.FirstName,
                    LastName = systemAdministrator.User.LastName
                };
            }
        }

        private async Task<PatientViewModel> GetPatientForRegistration(PatientRegistrationMode registrationMode, PatientViewModel? patient) => registrationMode switch
        {
            PatientRegistrationMode.Quick => await GetPatientForQuickRegistration().ConfigureAwait(false),
            PatientRegistrationMode.Manual => await GetPatientForManual(patient).ConfigureAwait(false),
            _ => null
        };
    }
}
