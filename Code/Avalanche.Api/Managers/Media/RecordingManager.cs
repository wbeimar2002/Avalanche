using Avalanche.Api.Services.Media;
using Ism.Recorder.Core.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class RecordingManager : IRecordingManager
    {
        readonly IRecorderService _recorderService;

        public RecordingManager(IRecorderService recorderService)
        {
            _recorderService = recorderService;
        }

        public async Task CaptureImage(CaptureImageRequest request)
        {
            await _recorderService.CaptureImage(request);
        }

        public async Task StartRecording(RecordMessage messsage)
        {
            await _recorderService.StartRecording(messsage);
        }

        public async Task StopRecording()
        {
            await _recorderService.StopRecording();
        }
    }
}
