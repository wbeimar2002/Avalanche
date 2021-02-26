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

        public async Task CaptureImage()
        {
            var now = DateTime.Now;
            var hacky_temp_libid_for_demo = $"{now.Year}_{now.Month}_{now.Day}T{now.Hour}_{now.Minute}_{now.Second}";
            var message = new CaptureImageRequest()
            {
                Record = new RecordMessage
                {
                    LibId = hacky_temp_libid_for_demo, // TODO: this is wrong and needs to come from the procedure
                    RepositoryId = "cache" // TODO: this is wrong and needs to come from the procedure
                },
            };
            await _recorderService.CaptureImage(message);
        }

        public async Task StartRecording()
        {
#warning TODO: determine sourcing of this info.
            //NOTE: it seems a bit awkward for the UI/API to need to know and/or generate this? Especially "libId"? 

            var now = DateTime.Now;
            var hacky_temp_libid_for_demo = $"{now.Year}_{now.Month}_{now.Day}T{now.Hour}_{now.Minute}_{now.Second}";

            var message = new RecordMessage
            {
                LibId = hacky_temp_libid_for_demo,
                RepositoryId = "cache"
            };

            await _recorderService.StartRecording(message);
        }

        public async Task StopRecording()
        {
            await _recorderService.StopRecording();
        }
    }
}
