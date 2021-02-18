using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Ism.PgsTimeout.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPgsTimeoutManager
    {
        Task<StateViewModel> GetPgsTimeoutMode();
        Task SetPgsTimeoutMode(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsVolume();
        Task SetPgsVolume(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsMute();
        Task SetPgsMute(StateViewModel requestViewModel);

        Task<StateViewModel> GetPgsPlaybackState();
        Task SetPgsPlaybackState(StateViewModel requestViewModel);

        Task<List<GreetingVideo>> GetPgsVideoFileList();
        Task<GreetingVideo> GetPgsVideoFile();
        Task SetPgsVideoFile(GreetingVideo video);

        Task SetPgsVideoPosition(StateViewModel requestViewModel);

        Task<StateViewModel> GetTimeoutPdfPath();
        Task<StateViewModel> GetTimeoutPageCount();
        Task<StateViewModel> GetTimeoutPage();
        Task SetTimeoutPage(StateViewModel requestViewModel);
        Task NextPage();
        Task PreviousPage();


        Task<StateViewModel> GetPgsStateForSink(SinkModel sink);
        Task SetPgsStateForSink(SinkStateViewModel sinkState);
        Task<List<VideoDeviceModel>> GetPgsSinks();
    }
}
