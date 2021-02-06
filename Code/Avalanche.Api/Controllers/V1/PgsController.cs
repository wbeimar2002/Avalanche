using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.PgsTimeout;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("room/[controller]")]
    [ApiController]
    [Authorize]
    public class PgsController : ControllerBase
    {
        private readonly ILogger<PgsController> _logger;
        private readonly IPgsTimeoutManager _pgsTimeoutManager;

        public PgsController(IPgsTimeoutManager pgsTimeoutManager, ILogger<PgsController> logger)
        {
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _pgsTimeoutManager = ThrowIfNullOrReturn(nameof(pgsTimeoutManager), pgsTimeoutManager);
        }

        #region PgsTimeoutPlayer methods

        /// <summary>
        /// Gets a collection of video files from the player
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("videoFiles")]
        public async Task<IActionResult> GetPgsVideoFiles([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsVideoFiles();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the current video file of the player
        /// </summary>
        /// <param name="file"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("videoFile")]
        public async Task<IActionResult> SetCurrentFile([FromBody] PgsVideoFile file, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsVideoFile(file);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the current position in video playback
        /// 0.0 means start of file, 1.0 means end of file
        /// </summary>
        /// <param name="position"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("videoPosition")]
        public async Task<IActionResult> SetPgsVideoPosition([FromBody] double position, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPlaybackPosition(position);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the volume of pgs audio
        /// 0.0 means mute, 1.0 means loudest. Note that 0.0 is different than muting
        /// </summary>
        /// <param name="level"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("volume/level")]
        public async Task<IActionResult> SetPgsVolume([FromBody] double level, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsVolume(level);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Gets the current pgs audio volume. Range is from 0-1
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("volume/level")]
        public async Task<IActionResult> GetPgsVolume([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var volume = await _pgsTimeoutManager.GetPgsVolume();
                return Ok(volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the PGS player audio mute
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("volume/mute")]
        public async Task<IActionResult> SetPgsMute([FromBody] bool mute, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsMute(mute);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Gets if the pgs player audio is muted
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("volume/mute")]
        public async Task<IActionResult> GetPgsMute([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var muted = await _pgsTimeoutManager.GetPgsMute();
                return Ok(muted);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #endregion

        #region API methods

        /// <summary>
        /// Starts or stops PGS mode
        /// </summary>
        /// <param name="pgsState"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("state")]
        public async Task<IActionResult> SetPgsState([FromBody] bool pgsState, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                // start or stop pgs based on the requested state
                // the pgsTimeoutManager deals with pgs-timeout interaction
                // it also deals with something like 2 UIs starting pgs at the same time
                if (pgsState)
                    await _pgsTimeoutManager.StartPgs();
                else
                    await _pgsTimeoutManager.StopPgs();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Gets the list of pgs sinks and their current checked state
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("sinks")]
        [Produces(typeof(List<VideoSink>))]
        public async Task<IActionResult> GetPgsSinks([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsSinks();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the checked state of a pgs sink and internally it gets broadcast
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="index"></param>
        /// <param name="enabled"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("sink/{alias}:{index}/state")]
        public async Task<IActionResult> SetPgsStateForSink(string alias, int index, [FromBody] bool enabled, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsStateForSink(new AliasIndexApiModel(alias, index), enabled);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Gets the checked state of a pgs sink
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="index"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("sink/{alias}:{index}/state")]
        public async Task<IActionResult> GetPgsStateForSink(string alias, int index, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var enabled = await _pgsTimeoutManager.GetPgsStateForSink(new AliasIndexApiModel(alias, index));
                return Ok(enabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #endregion

    }
}
