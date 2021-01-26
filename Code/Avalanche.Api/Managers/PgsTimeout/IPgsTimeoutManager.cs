﻿using Avalanche.Shared.Domain.Enumerations;
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

        /// <summary>
        /// Returns a list of PGS video files. Key is display name, value is file path
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetPgsVideoFiles();

        /// <summary>
        /// Sets the current video file of the PGS player
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task SetPgsVideoFile(string path);

        /// <summary>
        /// Sets the playback position to the specified value
        /// 0.0 means the beginning of the video, 1.0 means the end of the video
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Task SetPlaybackPosition(double position);

        /// <summary>
        /// Gets a collection of configured PGS displays
        /// </summary>
        /// <returns></returns>
        Task<IList<Output>> GetPgsOutputs();

        // TODO: implement volume control
        // will PGS volume go thru something like a virtual audio device? 
        // or can we simply set the volume on the MediaElement in the player?
        // same for muted
        // is PGS audio its own thing or can audio itself be an isolated element?
        Task SetPgsVolume(double volume);

        /// <summary>
        /// Sets the mode of the pgs timeout player
        /// Internally, this tells the player which elements should be visible or not
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// 
        // this is internal to the pgs player
        // pgs manager calls into wpf app to tell it this
        //Task SetPgsTimeoutPlayerMode(TimeoutModes mode);

        Task<List<Output>> GetTimeoutOutputs();
    }
}
