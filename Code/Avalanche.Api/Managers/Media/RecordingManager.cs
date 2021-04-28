using Avalanche.Api.Services.Media;
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

        public string GetCapturePreview(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
            var relative = GetRepositoryRelativePathFromProcedureId(procedureId);
            relative = Path.Combine(repository, relative, path);

            var translated = relative.Replace('\\', '/').TrimStart('/');

            return System.IO.Path.Combine(libraryRoot, translated);
        }

        public string GetCaptureVideo(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
            var relative = GetRepositoryRelativePathFromProcedureId(procedureId);
            relative = Path.Combine(repository, relative, path);
            
            var translated = relative.Replace('\\', '/').TrimStart('/');

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

        private string GetRepositoryRelativePathFromProcedureId(string procedureId)
        {
            var strYear = procedureId.Substring(0, 4);
            var strMonth = procedureId.Substring(5, 2);
            return Path.Combine(strYear, strMonth, procedureId);
        }
    }
}
