using Ism.Recorder.Core.V1.Protos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IRecorderService
    {
        Task<bool> IsRecording();
        Task StopRecording();
        Task<RecorderState> GetRecorderState();
        Task<IEnumerable<RecordChannelMessage>> GetRecordingChannels();
        Task FinishProcedure();
    }
}
