using Ism.Recorder.Client.V1;
using Ism.Recorder.Core.V1.Protos;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RecorderService : IRecorderService
    {
        private readonly RecorderSecureClient _client;

        public RecorderService(RecorderSecureClient client)
        {
            _client = client;
        }

        public async Task StartRecording(RecordMessage recordMessage) => await _client.StartRecording(recordMessage);

        public async Task<RecorderState> GetRecorderState()
        { 
            await _client.GetRecorderState();
            return new RecorderState()
            {
                State = 1,
            };
        }

        public async Task StopRecording() => await _client.StopRecording();

        public async Task CaptureImage(CaptureImageRequest captureRequest) => await _client.CaptureImage(captureRequest);
    }
}
