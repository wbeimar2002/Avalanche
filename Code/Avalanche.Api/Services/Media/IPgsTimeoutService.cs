using Ism.PgsTimeout.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    /// <summary>
    /// Interface to the pgs timeout player application
    /// </summary>
    public interface IPgsTimeoutService
    {
        Task<GetPgsTimeoutModeResponse> GetPgsTimeoutMode();
        Task SetPgsTimeoutMode(SetPgsTimeoutModeRequest request);

        Task<GetPgsVolumeResponse> GetPgsVolume();
        Task SetPgsVolume(SetPgsVolumeRequest request);
        
        Task<GetPgsMuteResponse> GetPgsMute();
        Task SetPgsMute(SetPgsMuteRequest request);

        Task<GetPgsPlaybackStateResponse> GetPgsPlaybackState();
        Task SetPgsPlaybackState(SetPgsPlaybackStateRequest request);

        Task<GetPgsVideoListResponse> GetPgsVideoFileList();
        Task<GetPgsVideoFileResponse> GetPgsVideoFile();
        Task SetPgsVideoFile(SetPgsVideoFileRequest request);

        Task SetPgsVideoPosition(SetPgsVideoPositionRequest request);


        
        Task<GetTimeoutPdfFileResponse> GetTimeoutPdfFileName();
        Task<GetTimeoutPageCountResponse> GetTimeoutPageCount();
        Task<GetTimeoutPageResponse> GetTimeoutPage();
        Task SetTimeoutPage(SetTimeoutPageRequest request);
        Task NextPage();
        Task PreviousPage();
    }
}
