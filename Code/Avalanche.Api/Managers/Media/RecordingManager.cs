using Avalanche.Api.Services.Media;
using Ism.Recorder.Core.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class RecordingManager : IRecordingManager
    {
        private readonly IStateClient _stateClient;
        private readonly IRecorderService _recorderService;

        public RecordingManager(IRecorderService recorderService, IStateClient stateClient)
        {
            _recorderService = recorderService;
            _stateClient = stateClient;
        }

        public async Task CaptureImage()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            if (null == activeProcedure)
            {
                throw new InvalidOperationException("No active procedure exists");
            }

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

#warning TODO: This is wrong and intended only for a workflow demo. Remove.
        // Need to define and implement correct image retrieval patterns. Not in scope of current work, but it is probably not correct to just accept any path string.
        public string GetCapturePreview(string path)
        {
            var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
            var translated = path.Replace('\\', '/').TrimStart('/');
            return System.IO.Path.Combine(libraryRoot, translated);           
        }
        
        // TODO: same as above. Need to define file retrieval patterns.
        public string GetCaptureVideo(string path)
        {
            var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
            var translated = path.Replace('\\', '/').TrimStart('/');
            return System.IO.Path.Combine(libraryRoot, translated);
        }

        public async Task StartRecording()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            if (null == activeProcedure)
            {
                throw new InvalidOperationException("No active procedure exists");
            }

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
    }
}
