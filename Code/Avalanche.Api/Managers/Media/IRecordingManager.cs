using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IRecordingManager
    {
        Task StartRecording();
        Task StopRecording();
        Task CaptureImage();
        Task<IEnumerable<RecordingChannelModel>> GetRecordingChannels();
        Task<RecordingTimelineViewModel?> GetRecordingTimelineByImageId(Guid imageId);
        Task CaptureImageFromVideo(Guid videoId, TimeSpan position);
    }
}
