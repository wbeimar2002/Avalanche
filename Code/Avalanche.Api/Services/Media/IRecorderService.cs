using Ism.Recorder.Core.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IRecorderService
    {
        Task StartRecording(RecordMessage recordMessage);
        Task StopRecording();

        Task CaptureImage(CaptureImageRequest captureRequest);
    }
}
