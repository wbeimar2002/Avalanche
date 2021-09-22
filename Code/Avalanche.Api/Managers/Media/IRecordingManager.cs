using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
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
        string GetCapturePreview(string path, string procedureId, string repository);
        string GetCaptureVideo(string path, string procedureId, string repository);
        Task<IEnumerable<RecordingChannelModel>> GetRecordingChannels();
        Task<RecordingTimelineViewModel> GetRecordingTimelineByImageId(Guid imageId);
        Task CaptureImageFromVideo(Guid videoId, TimeSpan position);
    }
}
