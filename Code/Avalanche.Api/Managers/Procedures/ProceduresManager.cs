using AutoMapper;
using Avalanche.Api.Managers.Data;
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
    public class ProceduresManager : IProceduresManager
    {
        private readonly IStateClient _stateClient;
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;

        private readonly IDataManager _dataManager;
        private readonly GeneralApiConfiguration _generalApiConfig;
        private readonly SetupConfiguration _setupConfiguration;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ProceduresManager(IStateClient stateClient,
            ILibraryService libraryService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IDataManager dataManager,
            GeneralApiConfiguration generalApiConfig,
            SetupConfiguration setupConfiguration)
        {
            _stateClient = stateClient;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _dataManager = dataManager;
            _generalApiConfig = generalApiConfig;
            _setupConfiguration = setupConfiguration;
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        public async Task<ActiveProcedureViewModel> GetActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var result = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            if (result != null)
                result.RecorderState = (int?)(await _recorderService.GetRecorderState().ConfigureAwait(false)).State;

            return result;
        }

        /// <summary>
        /// Set ActiveProcedure's "RequiresUserConfirmation" flag to false.
        /// </summary>
        public async Task ConfirmActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            activeProcedure.RequiresUserConfirmation = false;
            await _stateClient.PersistData(activeProcedure);
        }

        public async Task DeleteActiveProcedureMediaItem(ProcedureContentType procedureContentType, Guid contentId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

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

            await _libraryService.DeleteActiveProcedureMediaItem(request);
        }


        public async Task DeleteActiveProcedureMediaItems(ProcedureContentType procedureContentType, IEnumerable<Guid> contentIds)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

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

            await _libraryService.DeleteActiveProcedureMediaItems(request);
        }

        private static void ThrowIfVideoCannotBeDeleted(ActiveProcedureState activeProcedure, Guid videoContent)
        {
            var video = activeProcedure.Videos.Single(v => v.VideoId == videoContent);
            if (!video.VideoStopTimeUtc.HasValue)
            {
                throw new InvalidOperationException("Cannot delete video that is currently recording");
            }
        }

        public async Task DiscardActiveProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var request = _mapper.Map<ActiveProcedureState, DiscardActiveProcedureRequest>(activeProcedure);

            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _recorderService.FinishProcedure();
            await _libraryService.DiscardActiveProcedure(request);
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var request = _mapper.Map<ActiveProcedureState, CommitActiveProcedureRequest>(activeProcedure);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _recorderService.FinishProcedure();

            await _libraryService.CommitActiveProcedure(request);
        }

        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var response = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo),
                Clinical = true
            });

            return _mapper.Map<ProcedureAllocationViewModel>(response);
        }

        public async Task UpdateProcedure(ProcedureViewModel procedureViewModel)
        {
            Preconditions.ThrowIfNull(nameof(procedureViewModel), procedureViewModel);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.LibraryId), procedureViewModel.LibraryId);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.Patient.MRN), procedureViewModel.Patient.MRN);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.Patient.LastName), procedureViewModel.Patient.LastName);

            ValidateDynamicConditions(procedureViewModel);

            var procedure = _mapper.Map<ProcedureViewModel, ProcedureMessage>(procedureViewModel);

            await _libraryService.UpdateProcedure(new UpdateProcedureRequest
            {
                Procedure = procedure
            }).ConfigureAwait(false);
        }

        private void ValidateDynamicConditions(ProcedureViewModel procedure)
        {
            foreach (var item in _setupConfiguration.PatientInfo.Where(f => f.Required))
            {
                switch (item.Id)
                {
                    case "firstName":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.FirstName), procedure.Patient.FirstName);
                        break;
                    case "sex":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.Sex), procedure.Patient.Sex);
                        break;
                    case "dateOfBirth":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.DateOfBirth), procedure.Patient.DateOfBirth);
                        break;
                    case "physician":
                        Preconditions.ThrowIfNull(nameof(procedure.Physician), procedure.Physician);
                        break;
                    case "department":
                        Preconditions.ThrowIfNull(nameof(procedure.Department), procedure.Department);
                        break;
                    case "procedureType":
                        Preconditions.ThrowIfNull(nameof(procedure.ProcedureType), procedure.ProcedureType);
                        break;
                    case "accessionNumber":
                        Preconditions.ThrowIfNull(nameof(procedure.Accession), procedure.Accession);
                        break;
                    case "scopeSerialNumber":
                        Preconditions.ThrowIfNull(nameof(procedure.ScopeSerialNumber), procedure.ScopeSerialNumber);
                        break;
                    case "diagnosis":
                        Preconditions.ThrowIfNull(nameof(procedure.Diagnosis), procedure.Diagnosis);
                        break;
                    case "clinicalNotes":
                        Preconditions.ThrowIfNull(nameof(procedure.ClinicalNotes), procedure.ClinicalNotes);
                        break;
                    //case "procedureId":
                    //    Preconditions.ThrowIfNull(nameof(procedure.ProcedureId), procedure.ProcedureId);
                    //    break;
                }
            }
        }

        public async Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Page)} must be a positive integer greater than 0", filter.Page < 0);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be lower than {MinPageSize}", filter.Limit < MinPageSize);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be larger than {MaxPageSize}", filter.Limit > MaxPageSize);

            var libraryFilter = _mapper.Map<ProcedureSearchFilterViewModel, GetFinishedProceduresRequest>(filter);
            var response = await _libraryService.GetFinishedProcedures(libraryFilter).ConfigureAwait(false);

            return new ProceduresContainerViewModel()
            {
                TotalCount = response.TotalCount,
                Procedures = _mapper.Map<IList<ProcedureMessage>, IList<ProcedureViewModel>>(response.Procedures)
            };
        }

        public async Task<ProceduresContainerViewModel> SearchByPatient(string patientId)
        {
            Preconditions.ThrowIfNull(nameof(patientId), patientId);

            var response = await _libraryService.GetFinishedProceduresByPatient(new GetFinishedProceduresRequestByPatient()
            {
                PatientId = patientId
            }).ConfigureAwait(false);

            return new ProceduresContainerViewModel()
            {
                TotalCount = response.TotalCount,
                Procedures = _mapper.Map<IList<ProcedureMessage>, IList<ProcedureViewModel>>(response.Procedures)
            };
        }

        public async Task<ProcedureViewModel> GetProcedureDetails(string libraryId)
        {
            Preconditions.ThrowIfNull(nameof(libraryId), libraryId);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(libraryId), libraryId);

            var response = await _libraryService.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = libraryId
            }).ConfigureAwait(false);

            return _mapper.Map<ProcedureMessage, ProcedureViewModel>(response.Procedure);
        }

        public async Task ApplyLabelToActiveProcedure(ContentViewModel labelContent)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(labelContent.Label), labelContent.Label);            

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);

            // If adhoc labels allowed option enabled, add label to store
            if (_generalApiConfig.AdHocLabelsAllowed)
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
            if(labelModel == null || labelModel?.Id == 0)
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
            catch(InvalidOperationException ex)
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
    }
}
