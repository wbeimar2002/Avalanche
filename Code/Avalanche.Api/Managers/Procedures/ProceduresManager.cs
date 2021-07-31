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
        private readonly IRecorderService _recorderService;

        private readonly IDataManager _dataManager;
        private readonly GeneralApiConfiguration _generalApiConfig;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ProceduresManager(IStateClient stateClient, ILibraryService libraryService, IAccessInfoFactory accessInfoFactory, IMapper mapper, IRecorderService recorderService,
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
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var result = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            if (result != null)
                result.RecorderState = (await _recorderService.GetRecorderState()).State;

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

            var request = new DeleteActiveProcedureMediaRequest()
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

            if (await _recorderService.IsRecording())
                await _recorderService.StopRecording();

            await _libraryService.DiscardActiveProcedure(request);
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var request = _mapper.Map<ActiveProcedureState, CommitActiveProcedureRequest>(activeProcedure);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            if (await _recorderService.IsRecording())
                await _recorderService.StopRecording();

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
            var procedure = _mapper.Map<ProcedureViewModel, ProcedureMessage>(procedureViewModel);

            await _libraryService.UpdateProcedure(new UpdateProcedureRequest
            {
                Procedure = procedure
            });
        }

        public async Task<ProceduresContainerViewModel> BasicSearch(ProcedureSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Page)} must be a positive integer greater than 0", filter.Page < 0);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be lower than {MinPageSize}", filter.Limit < MinPageSize);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be larger than {MaxPageSize}", filter.Limit > MaxPageSize);

            var response = await _libraryService.GetFinishedProcedures(_mapper.Map<ProcedureSearchFilterViewModel, GetFinishedProceduresRequest>(filter));

            return new ProceduresContainerViewModel()
            {
                TotalCount = response.TotalCount,
                Procedures = _mapper.Map<IList<ProcedureMessage>, IList<ProcedureViewModel>>(response.Procedures)
            };
        }

        public async Task<ProceduresContainerViewModel> AdvancedSearch(ProcedureAdvancedSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Page)} must be a positive integer greater than 0", filter.Page < 0);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be lower than {MinPageSize}", filter.Limit < MinPageSize);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be larger than {MaxPageSize}", filter.Limit > MaxPageSize);

            var libraryFilter = _mapper.Map<ProcedureAdvancedSearchFilterViewModel, GetFinishedProceduresRequest>(filter);
            var response = await _libraryService.GetFinishedProcedures(libraryFilter);

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
            });

            return _mapper.Map<ProcedureMessage, ProcedureViewModel>(response.Procedure);
        }

        public async Task ApplyLabelToActiveProcedure(ContentViewModel labelContent)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(labelContent.Label), labelContent.Label);            

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            // If adhoc labels allowed option enabled, add label to store
            if (_generalApiConfig.AdHocLabelsAllowed)
            {
                var newLabel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id);
                if (newLabel == null || newLabel?.Id == 0)
                {
                    await _dataManager.AddLabel(new LabelModel
                    {
                        Name = labelContent.Label,
                        ProcedureTypeId = activeProcedure.ProcedureType?.Id
                    });
                }
            }

            //check label exist in store before associating the label to active procedure
            var labelModel = await _dataManager.GetLabel(labelContent.Label, activeProcedure.ProcedureType?.Id);
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
            });
        }
    }
}
