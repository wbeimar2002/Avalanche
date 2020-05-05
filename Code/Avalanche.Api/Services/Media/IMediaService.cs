using Avalanche.Shared.Domain.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        //Video
        Task<CommandResponse> PlayVideoAsync(Command command);
        Task<CommandResponse> HandleMessageAsync(Command command);
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
