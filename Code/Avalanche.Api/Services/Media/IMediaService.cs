using Avalanche.Shared.Domain.Models;
using Ism.Streaming.Common.Core;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        WebRtcStreamer.WebRtcStreamerClient Client { get; set; }

        //Video
        Task<CommandResponse> PlayVideoAsync(Command command);
        Task<CommandResponse> HandleMessageForVideoAsync(Command command);
        Task<CommandResponse> StopVideoAsync(Command command);

        //Audio
        Task<CommandResponse> PlayAudioAsync(Command command);
        Task<CommandResponse> StopAudioAsync(Command command);
        Task<CommandResponse> MuteAudioAsync(Command command);
        Task<CommandResponse> GetVolumeUpAsync(Command command);
        Task<CommandResponse> GetVolumeDownAsync(Command command);

        //Timeout PDF
        Task<CommandResponse> PlaySlidesAsync(Command command);
        Task<CommandResponse> StopSlidesAsync(Command command);
        Task<CommandResponse> NextSlideAsync(Command command);
        Task<CommandResponse> PreviousSlideAsync(Command command);
    }
}
