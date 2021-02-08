using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.PgsTimeout
{
    public interface IPgsTimeoutManager
    {
        /// <summary>
        /// Sends PGS to all displays and puts the room into PGS mode
        /// </summary>
        /// <returns></returns>
        Task StartPgs();

        /// <summary>
        /// Restores previously saved routes and takes the room out of PGS mode
        /// </summary>
        /// <returns></returns>
        Task StopPgs();


        #region PgsTimeout player methods

        /// <summary>
        /// Returns a list of PGS video files
        /// </summary>
        /// <returns></returns>
        Task<IList<PgsVideoFile>> GetPgsVideoFiles();

        /// <summary>
        /// Sets the current video file of the PGS player
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task SetPgsVideoFile(PgsVideoFile path);

        /// <summary>
        /// Sets the playback position to the specified value
        /// 0.0 means the beginning of the video, 1.0 means the end of the video
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Task SetPlaybackPosition(double position);

        /// <summary>
        /// Gets the current pgs volume level. Range is from 0-1
        /// </summary>
        /// <returns></returns>
        Task<double> GetPgsVolume();

        /// <summary>
        /// Sets the pgs player volume level. Range is from 0-1
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        Task SetPgsVolume(double volume);

        /// <summary>
        /// Returns true if PGS is muted
        /// </summary>
        /// <returns></returns>
        Task<bool> GetPgsMute();

        /// <summary>
        /// Set to true to mute PGS audio
        /// </summary>
        /// <param name="mute"></param>
        /// <returns></returns>
        Task SetPgsMute(bool mute);

        #endregion

        #region api methods

        /// <summary>
        /// Returns true/false if the specified display is checked
        /// </summary>
        /// <param name="displayId"></param>
        /// <returns></returns>
        Task<bool> GetPgsStateForSink(AliasIndexApiModel displayId);

        /// <summary>
        /// Should map to checked/unchecked when toggling PGS from a display
        /// </summary>
        /// <param name="displayId"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        Task SetPgsStateForSink(AliasIndexApiModel displayId, bool enabled);

        /// <summary>
        /// Gets a collection of configured PGS displays
        /// </summary>
        /// <returns></returns>
        Task<IList<VideoSink>> GetPgsSinks();

        // TODO: determine if this can be removed, timeout displays aren't changed at runtime
        /// <summary>
        /// Gets a collection of configured timeout sinks
        /// </summary>
        /// <returns></returns>
        Task<IList<VideoSink>> GetTimeoutSinks();

        #endregion
    }
}
