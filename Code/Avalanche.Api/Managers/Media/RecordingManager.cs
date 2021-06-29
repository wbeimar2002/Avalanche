using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Recorder.Core.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Ism.Utility.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class RecordingManager : IRecordingManager
    {
        private readonly IStateClient _stateClient;
        private readonly IRecorderService _recorderService;
        private readonly IMapper _mapper;

        public RecordingManager(IRecorderService recorderService, IStateClient stateClient, IMapper mapper)
        {
            _recorderService = recorderService;
            _stateClient = stateClient;
            _mapper = mapper;
        }

        public async Task CaptureImage()
        {
            var activeProcedure = await GetActiveProcedureState();
            var message = new CaptureImageRequest()
            {
                Record = new RecordMessage
                {
                    LibId = activeProcedure.LibraryId, 
                    RepositoryId = activeProcedure.RepositoryId
                },
            };
            await _recorderService.CaptureImage(message);
        }

        public string GetCapturePreview(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            return ProceduresHelper.GetRelativePath(procedureId, repository, path);
        }

        public string GetCaptureVideo(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            return ProceduresHelper.GetRelativePath(procedureId, repository, path);
        }

        public async Task<IEnumerable<RecordingChannelModel>> GetRecordingChannels()
        {
            var channels = await _recorderService.GetRecordingChannels();
            return _mapper.Map<IEnumerable<RecordChannelMessage>, IEnumerable<RecordingChannelModel>>(channels).ToList();
        }

        public async Task<RecordingTimelineViewModel> GetRecordingTimelineByImageId(Guid imageId)
        {
            Preconditions.ThrowIfNullOrDefault(nameof(imageId), imageId);

            var activeProcedure = await GetActiveProcedureState();
            var recEvent = activeProcedure.RecordingEvents.Find(x => x.ImageId.Equals(imageId));
            if (recEvent == null)
                return null;

            var timelineModel = new RecordingTimelineModel { VideoId = recEvent.VideoId, VideoOffset = recEvent.VideoOffset };

            return _mapper.Map<RecordingTimelineModel, RecordingTimelineViewModel>(timelineModel);
        }

        public async Task StartRecording()
        {
            var activeProcedure = await GetActiveProcedureState();
            var message = new RecordMessage
            {
                LibId = activeProcedure.LibraryId,
                RepositoryId = activeProcedure.RepositoryId
            };

            await _recorderService.StartRecording(message);
        }

        public async Task StopRecording()
        {
            await _recorderService.StopRecording();
        }

        private async Task<ActiveProcedureState> GetActiveProcedureState()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            if (null == activeProcedure)
            {
                throw new InvalidOperationException("No active procedure exists");
            }
            return activeProcedure;
        }
    }
}
