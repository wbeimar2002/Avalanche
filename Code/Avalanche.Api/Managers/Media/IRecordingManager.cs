using Ism.Recorder.Core.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IRecordingManager
    {
        Task StartRecording();

        Task StopRecording();

        Task CaptureImage();

        string GetCapturePreview(string path);
    }
}
