using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
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

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ProceduresManager(IStateClient stateClient, ILibraryService libraryService, IAccessInfoFactory accessInfoFactory, IMapper mapper, IRecorderService recorderService)
        {
            _stateClient = stateClient;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _recorderService = recorderService;
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

        public async Task DeleteActiveProcedureMedia(ProcedureContentType procedureContentType, Guid contentId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            if (procedureContentType == ProcedureContentType.Video)
            {
                var video = activeProcedure.Videos.Single(v => v.VideoId == contentId);
                if (!video.VideoStopTimeUtc.HasValue)
                {
                    throw new InvalidOperationException("Cannot delete video that is currently recording");
                }
            }

            var request = new DeleteActiveProcedureMediaRequest()
            {
                ContentId = contentId.ToString(),
                ContentType = _mapper.Map<ContentType>(procedureContentType),
                ProcedureId = _mapper.Map<ProcedureIdMessage>(activeProcedure),
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo)
            };

            await _libraryService.DeleteActiveProcedureMedia(request);
        }


        public async Task DeleteActiveProcedureMediaItems(ProcedureContentType procedureContentType, IEnumerable<Guid> contentIds)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            if (procedureContentType == ProcedureContentType.Video)
            {
                foreach (var videoContent in contentIds)
                {
                    var video = activeProcedure.Videos.Single(v => v.VideoId == videoContent);
                    if (!video.VideoStopTimeUtc.HasValue)
                    {
                        throw new InvalidOperationException("Cannot delete video that is currently recording");
                    }
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

        public async Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter)
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

        public async Task ApplyLabelToActiveProcedure(LabelContentViewModel labelContent)
        {
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(labelContent.Label), labelContent.Label);
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

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
