using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPgsManager
    {
        Task<StateViewModel> GetPgsTimeoutMode();
        Task SetPgsTimeoutMode(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsVolume();
        Task SetPgsVolume(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsMute();
        Task SetPgsMute(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsPlaybackState();
        Task SetPgsState(StateViewModel requestViewModel);

        Task<List<GreetingVideoModel>> GetPgsVideoFileList();
        Task<GreetingVideoModel> GetPgsVideoFile();
        Task SetPgsVideoFile(GreetingVideoModel video);

        Task SetPgsVideoPosition(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsStateForSink(SinkModel sink);
        Task SetPgsStateForSink(SinkStateViewModel sinkState);
        Task<IList<VideoSinkModel>> GetPgsSinks();
    }
}
