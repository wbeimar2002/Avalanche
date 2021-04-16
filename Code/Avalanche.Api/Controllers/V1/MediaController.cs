using Avalanche.Api.Managers.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PgsTimeout.V1.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MediaController : ControllerBase
    {
        readonly ILogger<MediaController> _logger;
        readonly IPgsManager _pgsManager;
        readonly ITimeoutManager _timeoutManager;
        readonly IWebHostEnvironment _environment;

        public MediaController(ILogger<MediaController> logger, IPgsManager pgsManager, ITimeoutManager timeoutManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _pgsManager = ThrowIfNullOrReturn(nameof(pgsManager), pgsManager);
            _timeoutManager = ThrowIfNullOrReturn(nameof(timeoutManager), timeoutManager);
        }


        /// <summary>
        /// Sets mode for PGS Timeout Player
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("mode/{mode}")]
        public async Task<IActionResult> SetMode(PgsTimeoutModeEnum mode, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsTimeoutMode(new StateViewModel() { Value = mode.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #region Routing

        /// <summary>
        /// Gets the list of pgs sinks and their current checked state
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("pgs/sinks")]
        [Produces(typeof(List<VideoSinkModel>))]
        public async Task<IActionResult> GetPgsSinks([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsManager.GetPgsSinks();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the checked state of a pgs sink and internally it gets broadcast
        /// </summary>
        /// <param name="sinkState"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("pgs/sinks/state")]
        public async Task<IActionResult> SetPgsStateForSink([FromBody]SinkStateViewModel sinkState)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsStateForSink(sinkState);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        [HttpGet("pgs/sinks/state")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsStateForSink([FromQuery] string alias, [FromQuery] string index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsManager.GetPgsStateForSink(new SinkModel()
                {
                    Alias = alias,
                    Index = index
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #endregion Routing

        #region PGS
        /// <summary>
        /// Sets the volume of pgs audio
        /// 0.0 means mute, 1.0 means loudest. Note that 0.0 is different than muting
        /// </summary>
        /// <param name="level"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("pgs/volume/level/{level}")]
        public async Task<IActionResult> SetPgsVolume(double level)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsVolume(new StateViewModel() { Value = level.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        [HttpGet("pgs/volume/level")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsVolume([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsManager.GetPgsVolume();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        [HttpPut("pgs/volume/mute/{muteState}")]
        public async Task<IActionResult> SetPgsMute(bool muteState)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsMute(new StateViewModel() { Value = muteState.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        [HttpGet("pgs/volume")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsMute([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsManager.GetPgsMute();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Sets the current video file of the player
        /// </summary>
        /// <param name="greetingVideo"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("pgs/files/videos")]
        public async Task<IActionResult> SetCurrentGreetingVideo([FromBody]GreetingVideoModel greetingVideo)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsVideoFile(greetingVideo);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Gets a collection of video files from the player
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("pgs/files/videos")]
        [Produces(typeof(List<GreetingVideoModel>))]
        public async Task<IActionResult> GetPgsVideoFiles([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsManager.GetPgsVideoFileList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        [HttpPut("pgs/currentvideo/position/{position}")]
        public async Task<IActionResult> SetPgsVideoPosition(double position)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsVideoPosition(new StateViewModel() { Value = position.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Starts or stops PGS mode
        /// </summary>
        /// <param name="state"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("pgs/state/{state}")]
        public async Task<IActionResult> SetPgsState(bool state)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsManager.SetPgsState(new StateViewModel() { Value = state.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #endregion

        #region Timeout
        /// <summary>
        /// Request Next Page for Timeout
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("timeout/pages/next")]
        public async Task<IActionResult> NextPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _timeoutManager.NextPage();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Request Previous Page for Timeout
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("timeout/pages/previous")]
        public async Task<IActionResult> PreviousPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _timeoutManager.PreviousPage();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Set current page number for timeout
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("timeout/pages/{pageNumber}")]
        public async Task<IActionResult> SetPage(string pageNumber)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _timeoutManager.SetTimeoutPage(new StateViewModel() { Value = pageNumber.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Get current page in timeout file
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("timeout/pages/current")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetCurrentPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _timeoutManager.GetTimeoutPage();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Get pages count from timeout file 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("timeout/pages/count")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPagesCount([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _timeoutManager.GetTimeoutPageCount();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Get timeout's file path
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("timeout/file/path")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetFilePath([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _timeoutManager.GetTimeoutPdfPath();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Starts or stops Timeout mode
        /// </summary>
        /// <param name="state"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("timeout/state/{state}")]
        public async Task<IActionResult> SetTimeoutState(bool state, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _timeoutManager.SetTimeoutState(new StateViewModel() { Value = state.ToString() });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Deactivates timeout when user moves out of timeout page
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("timeout/deactivate")]
        public async Task<IActionResult> DeActivateTimeout([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _timeoutManager.DeActivateTimeout();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
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
