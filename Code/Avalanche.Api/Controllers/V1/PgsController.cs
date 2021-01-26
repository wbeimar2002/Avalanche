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
    //[Authorize]
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

        [HttpGet("videoFiles")]
        [Produces(typeof(IDictionary<string, string>))]
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

        [HttpPut("videoFile")]
        public async Task<IActionResult> SetCurrentFile(string file, [FromServices] IWebHostEnvironment env)
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
        public async Task<IActionResult> SetPgsVideoPosition(double position, [FromServices] IWebHostEnvironment env)
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
        /// 0.0 means mute, 1.0 means loudest
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("volume")]
        public async Task<IActionResult> SetPgsVolume(double volume, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _pgsTimeoutManager.SetPgsVolume(volume);

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

        #endregion

        #region API methods

        /// <summary>
        /// Starts or stops PGS mode
        /// </summary>
        /// <param name="pgsState"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("state")]
        public async Task<IActionResult> SetPgsState(bool pgsState, [FromServices] IWebHostEnvironment env)
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

        [HttpGet("outputs")]
        [Produces(typeof(List<Output>))]
        public async Task<IActionResult> GetPgsOutputs([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsOutputs();
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

        #endregion

    }
}
