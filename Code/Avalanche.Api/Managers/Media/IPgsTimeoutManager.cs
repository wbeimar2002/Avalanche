using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;
using Ism.SystemState.Models.PgsTimeout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IPgsTimeoutManager
    {
        /// <summary>
        /// Gets a value representing if the room is in pgs mode, timeout mode, or none
        /// </summary>
        /// <returns></returns>
        Task<PgsTimeoutRoomState> GetRoomState();

        /// <summary>
        /// Gets if timeout is a pdf file or a video source. 
        /// Useful for determining if the frontend needs to load a pdf or show the webrtc preview
        /// </summary>
        /// <returns></returns>
        Task<TimeoutModes> GetTimeoutMode();

        /// <summary>
        /// Starts pgs and sends pgs to all displays
        /// </summary>
        /// <returns></returns>
        Task StartPgs();

        /// <summary>
        /// Gets the checked state of all pgs displays
        /// </summary>
        /// <returns></returns>
        Task<IList<PgsSinkStateModel>> GetPgsStateForSinks();

        /// <summary>
        /// Sets the checked/enabled state for a single pgs display
        /// </summary>
        /// <param name="sinkState"></param>
        /// <returns></returns>
        Task SetPgsStateForSink(PgsSinkStateViewModel sinkState);

        /// <summary>
        /// Gets a collection of displays to be shown on the pgs tab
        /// The model type here is the same as video routing
        /// </summary>
        /// <returns></returns>
        Task<IList<VideoSinkModel>> GetPgsSinks();

        /// <summary>
        /// Stops both pgs and timeout
        /// </summary>
        /// <returns></returns>
        Task StopPgsAndTimeout();

        /// <summary>
        /// Starts timeout
        /// </summary>
        /// <returns></returns>
        Task StartTimeout();

        #region pass through to player
        // these methods are simply passed onto the player application

        /// <summary>
        /// Gets a list of video files for the player.
        /// <see cref="GreetingVideoModel.Index"/> is the key for selecting a video file
        /// </summary>
        /// <returns></returns>
        Task<List<GreetingVideoModel>> GetPgsVideoFileList();

        /// <summary>
        /// Gets the current playing video file
        /// </summary>
        /// <returns></returns>
        Task<GreetingVideoModel> GetPgsVideoFile();

        /// <summary>
        /// Sets the current video file.
        /// <see cref="GreetingVideoModel.Index"/> is used for this
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        Task SetPgsVideoFile(GreetingVideoModel video);

        /// <summary>
        /// Sets the playback position of the current video file. 0.0 means rewind to beginning, 0.5 means halfway, etc
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Task SetPgsVideoPosition(double position);

        /// <summary>
        /// Gets the pgs volume level. Range is 0.0 to 1.0
        /// </summary>
        /// <returns></returns>
        Task<double> GetPgsVolume();

        /// <summary>
        /// Sets the pgs volume level. Range is 0.0 to 1.0
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        Task SetPgsVolume(double volume);

        /// <summary>
        /// Gets the muted state of the pgs video
        /// </summary>
        /// <returns></returns>
        Task<bool> GetPgsMute();

        /// <summary>
        /// Sets the muted state of the pgs video
        /// </summary>
        /// <param name="isMuted"></param>
        /// <returns></returns>
        Task SetPgsMute(bool isMuted);

        /// <summary>
        /// Returns true if video is playing. Useful for the contextual controls
        /// </summary>
        /// <returns></returns>
        Task<bool> GetPgsVideoPlaybackState();

        /// <summary>
        /// Plays/pauses the current video file. This is not the same as starting pgs
        /// </summary>
        /// <param name="isPlaying"></param>
        /// <returns></returns>
        Task SetPgsVideoPlaybackState(bool isPlaying);

        /// <summary>
        /// Gets the filename of the timeout pdf
        /// </summary>
        /// <returns></returns>
        Task<string> GetTimeoutPdfFileName();

        /// <summary>
        /// Gets how many pages are in the timeout pdf
        /// </summary>
        /// <returns></returns>
        Task<int> GetTimeoutPageCount();

        /// <summary>
        /// Gets the current timeout page number
        /// Useful for loading the timeout tab mid-timeout
        /// </summary>
        /// <returns></returns>
        Task<int> GetTimeoutPage();

        /// <summary>
        /// Sets the current timeout page
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        Task SetTimeoutPage(int pageNumber);

        /// <summary>
        /// Goes to the next page of the timeout pdf
        /// Wraps around to the beginning of on the last page
        /// </summary>
        /// <returns></returns>
        Task NextPage();

        /// <summary>
        /// Goes to the previous page of the timeout pdf
        /// Wraps around to the end if on the first page
        /// </summary>
        /// <returns></returns>
        Task PreviousPage();

        /// <summary>
        /// Sets the pgs/timeout player to the specified mode.
        /// Note that this is not the room state
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task SetPgsTimeoutPlayerMode(PgsTimeoutModes mode);

        /// <summary>
        /// Gets the current mode of the player application
        /// Note that this is not the room state
        /// </summary>
        /// <returns></returns>
        Task<PgsTimeoutModes> GetPgsTimeoutPlayerMode();

        #endregion

    }
}
