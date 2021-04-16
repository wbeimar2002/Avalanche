using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPgsManager
    {
        Task<TimeoutModes> GetPgsTimeoutMode();
        Task SetPgsTimeoutMode(PgsTimeoutModes mode);

        Task<double> GetPgsVolume();
        Task SetPgsVolume(double volume);

        Task<bool> GetPgsMute();
        Task SetPgsMute(bool isMuted);

        Task<bool> GetPgsPlaybackState();
        Task StartPgs();
        Task StopPgs();

        Task<List<GreetingVideoModel>> GetPgsVideoFileList();
        Task<GreetingVideoModel> GetPgsVideoFile();
        Task SetPgsVideoFile(GreetingVideoModel video);

        Task SetPgsVideoPosition(double position);

        Task<bool> GetPgsStateForSink(SinkModel sink);
        Task SetPgsStateForSink(SinkStateViewModel sinkState);
        Task<IList<VideoSinkModel>> GetPgsSinks();

        Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetPrePgsRoutes();
    }
}
