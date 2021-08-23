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
            switch ((Avalanche.Shared.Domain.Enumerations.RecorderState)(await _client.GetRecorderState().ConfigureAwait(false)).State)
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

        public RecorderService(RecorderSecureClient client) => _client = client;

        public async Task<RecorderState> GetRecorderState() => await _client.GetRecorderState().ConfigureAwait(false);

        public async Task StopRecording() => await _client.StopRecording().ConfigureAwait(false);

        public async Task<IEnumerable<RecordChannelMessage>> GetRecordingChannels()
        {
            var response = await _client.GetRecordingChannels().ConfigureAwait(false);
            return response.RecordingChannels.ToList();
        }

        public async Task FinishProcedure() => await _client.FinishProcedure().ConfigureAwait(false);
    }
}
