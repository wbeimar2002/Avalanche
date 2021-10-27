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
    public class ActiveProcedureManager : IActiveProcedureManager
    {
        private readonly IStateClient _stateClient;
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IRecorderService _recorderService;

        private readonly IDataManager _dataManager;
        private readonly GeneralApiConfiguration _generalApiConfig;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ActiveProcedureManager(IStateClient stateClient, ILibraryService libraryService, IAccessInfoFactory accessInfoFactory,
            IMapper mapper, IRecorderService recorderService,
            IDataManager dataManager, GeneralApiConfiguration generalApiConfig)
        {
            _stateClient = stateClient;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _recorderService = recorderService;
            _dataManager = dataManager;
            _generalApiConfig = generalApiConfig;
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
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>().ConfigureAwait(false);
            var request = _mapper.Map<ActiveProcedureState, CommitActiveProcedureRequest>(activeProcedure);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _recorderService.FinishProcedure().ConfigureAwait(false);

            await _libraryService.CommitActiveProcedure(request).ConfigureAwait(false);
        }

        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var response = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo),
                Clinical = true
            }).ConfigureAwait(false);

            return _mapper.Map<ProcedureAllocationViewModel>(response);
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

        private void ThrowIfVideoCannotBeDeleted(ActiveProcedureState activeProcedure, Guid videoContent)
        {
            var video = activeProcedure.Videos.Single(v => v.VideoId == videoContent);
            if (!video.VideoStopTimeUtc.HasValue)
            {
                throw new InvalidOperationException("Cannot delete video that is currently recording");
            }
        }
    }
}
