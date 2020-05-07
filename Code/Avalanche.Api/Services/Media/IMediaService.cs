using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.Common.Core;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }
        PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        //Video
        Task<CommandResponse> PgsPlayVideoAsync(Command command);
        Task<CommandResponse> PgsHandleMessageForVideoAsync(Command command);
        Task<CommandResponse> PgsStopVideoAsync(Command command);

        //Audio
        Task<CommandResponse> PgsPlayAudioAsync(Command command);
        Task<CommandResponse> PgsStopAudioAsync(Command command);
        Task<CommandResponse> PgsMuteAudioAsync(Command command);
        Task<CommandResponse> PgsGetAudioVolumeUpAsync(Command command);
        Task<CommandResponse> PgsGetAudioVolumeDownAsync(Command command);

        //Timeout PDF
        Task<CommandResponse> TimeoutSetModeAsync(Command command);
        Task<CommandResponse> TimeoutSetPageAsync(Command command);
        Task<CommandResponse> TimeoutNextSlideAsync(Command command);
        Task<CommandResponse> TimeoutPreviousSlideAsync(Command command);
    }
}
