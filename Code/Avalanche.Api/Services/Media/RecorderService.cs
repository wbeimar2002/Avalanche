using Ism.Recorder.Client.V1;
using Ism.Recorder.Core.V1.Protos;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RecorderService : IRecorderService
    {
        private readonly RecorderSecureClient _client;

        public async Task<bool> IsRecording()
        {
            var recorderState = (Avalanche.Shared.Domain.Enumerations.RecorderState)(await _client.GetRecorderState()).State;
            switch (recorderState)
            {
                case Avalanche.Shared.Domain.Enumerations.RecorderState.proc_recording_mov:
                case Avalanche.Shared.Domain.Enumerations.RecorderState.proc_recording_mov_and_pm:
                case Avalanche.Shared.Domain.Enumerations.RecorderState.proc_recording_pm:
                case Avalanche.Shared.Domain.Enumerations.RecorderState.proc_saving:
                    return true;
                default:
                    return false;
            }
        }

        public RecorderService(RecorderSecureClient client)
        {
            _client = client;
        }

        public async Task StartRecording(RecordMessage recordMessage) => await _client.StartRecording(recordMessage);

        public async Task<RecorderState> GetRecorderState() => await _client.GetRecorderState();

        public async Task StopRecording() => await _client.StopRecording();

        public async Task CaptureImage(CaptureImageRequest captureRequest) => await _client.CaptureImage(captureRequest);

        public async Task<IEnumerable<RecordChannelMessage>> GetRecordingChannels()
        {
            var response = await _client.GetRecordingChannels();
            return response.RecordingChannels.ToList();
        }

        public async Task FinishProcedure() => await _client.FinishProcedure();
    }
}
