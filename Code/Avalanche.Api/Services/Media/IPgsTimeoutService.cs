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
        /// <summary>
        /// Tells the player to go into idle mode, pgs mode, or timeout mode
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetPgsTimeoutMode(SetPgsTimeoutModeRequest request);

        /// <summary>
        /// Gets the current mode from the player
        /// </summary>
        /// <returns></returns>
        Task<GetPgsTimeoutModeResponse> GetPgsTimeoutMode();

        /// <summary>
        /// Gets the currently playing video file
        /// </summary>
        /// <returns></returns>
        Task<GetPgsVideoFileResponse> GetPgsVideoFile();

        /// <summary>
        /// Gets a collection of all configured video files
        /// </summary>
        /// <returns></returns>
        Task<GetPgsVideoListResponse> GetPgsVideoFileList();

        /// <summary>
        /// Sets the current video file and starts playing it
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetPgsVideoFile(SetPgsVideoFileRequest request);

        //TODO replace with SetVideoPosition(double)
        Task SetCurrentVideoToRandomTime(RandomPosRequest request);

        // TODO add play/pause endpoint

        /// <summary>
        /// Gets the current page number of the timeout pdf
        /// </summary>
        /// <returns></returns>
        Task<GetTimeoutPageResponse> GetTimeoutPage();

        /// <summary>
        /// Sets the current timeout pdf page
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetTimeoutPage(SetTimeoutPageRequest request);

        /// <summary>
        /// Gets the total number of pages in the timeout pdf
        /// </summary>
        /// <returns></returns>
        Task<GetTimeoutPageCountResponse> GetTimeoutPageCount();

        // TODO: work out file path funtime
        // pgs player "owns" the page number of the timeout pdf
        // web app needs either an absolute or relative path to this pdf
        /// <summary>
        /// Gets the file path to the timeout pdf
        /// </summary>
        /// <returns></returns>
        Task<GetTimeoutPdfPathResponse> GetTimeoutPdfPath();

        /// <summary>
        /// Advances to the next page in the timeout pdf
        /// Wraps around to the beginning if on the last page
        /// </summary>
        /// <returns></returns>
        Task NextPage();

        /// <summary>
        /// Navigates to the previous page in the timeout pdf
        /// Wraps around to the end if on the first page
        /// </summary>
        /// <returns></returns>
        Task PreviousPage();   
    }
}
