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

        private readonly IDataManager _dataManager;
        private readonly LabelsConfiguration _labelsConfig;

        // TODO: remove this when we figure out how to clean up dependencies
        private readonly IRoutingManager _routingManager;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ActiveProcedureManager(IStateClient stateClient, ILibraryService libraryService, IAccessInfoFactory accessInfoFactory,
            IMapper mapper, IRecorderService recorderService, IDataManager dataManager, LabelsConfiguration labelsConfig, IPatientsManager patientsManager, IDataManagementService dataManagementService, IRoutingManager routingManager)
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

        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure(Shared.Infrastructure.Enumerations.RegistrationMode registrationMode, PatientViewModel? patient = null)
        {
            if (registrationMode == Shared.Infrastructure.Enumerations.RegistrationMode.Quick)
            {
                patient = await _patientsManager.QuickPatientRegistration();
            }
            else
            {
                if (registrationMode == Shared.Infrastructure.Enumerations.RegistrationMode.Update)
                {
                    await _patientsManager.UpdatePatient(patient).ConfigureAwait(false);
                }
                else
                {
                    patient = await _patientsManager.RegisterPatient(patient).ConfigureAwait(false);
                }

                await CheckProcedureType(patient.ProcedureType, patient.Department).ConfigureAwait(false);
            }

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var response = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo),
                Clinical = true
            }).ConfigureAwait(false);

            var procedure = _mapper.Map<ProcedureAllocationViewModel>(response);
            var patientListSource = await _patientsManager.GetPatientListSource().ConfigureAwait(false);

            await PublishPersistData(patient, procedure, patientListSource, (int)registrationMode).ConfigureAwait(false);

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

        public async Task UpdateActiveProcedure(PatientViewModel patient)
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var model = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            _ = await _stateClient.UpdateData<ActiveProcedureState>(s => s.Replace(_ => model.Patient, patient)).ConfigureAwait(false);
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
                BackgroundRecordingMode = _mapper.Map<BackgroundRecordingMode>(patient.BackgroundRecordingMode),
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
    }
}
