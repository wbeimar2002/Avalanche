using Avalanche.Api.Managers.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
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
    public class RoomController : ControllerBase
    {
        readonly ILogger<RoomController> _logger;
        readonly IPgsTimeoutManager _pgsTimeoutManager;

        public RoomController(ILogger<RoomController> logger, IPgsTimeoutManager pgsTimeoutManager)
        {
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _pgsTimeoutManager = ThrowIfNullOrReturn(nameof(pgsTimeoutManager), pgsTimeoutManager);
        }

        #region Routing

        [HttpGet("pgs/sinks")]
        [Produces(typeof(List<VideoDeviceModel>))]
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

        [HttpPut("pgs/sinks/state")]
        public async Task<IActionResult> SetPgsStateForSink([FromBody]SinkStateViewModel sinkState, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsStateForSink(sinkState);
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

        [HttpGet("pgs/sinks/state")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsStateForSink([FromQuery] string alias, [FromQuery] int index, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsStateForSink(new SinkModel()
                {
                    Alias = alias,
                    Index = index
                });

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

        #endregion Routing

        #region PGS

        [HttpPut("volume/level/{level}")]
        public async Task<IActionResult> SetPgsVolume(double level, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsVolume(new StateViewModel() { Value = level.ToString() });
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

        [HttpGet("volume/level")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsVolume([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsVolume();
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

        [HttpPut("volume/mute/{muteState}")]
        public async Task<IActionResult> SetPgsMute(bool muteState, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsMute(new StateViewModel() { Value = muteState.ToString() });
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

        [HttpGet("volume")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPgsMute([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsMute();
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

        [HttpPut("pgs/files/videos")]
        public async Task<IActionResult> SetCurrentGreetingVideo([FromBody]GreetingVideoModel greetingVideo, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsVideoFile(greetingVideo);
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

        [HttpGet("pgs/files/videos")]
        [Produces(typeof(List<GreetingVideoModel>))]
        public async Task<IActionResult> GetPgsVideoFiles([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetPgsVideoFileList();
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

        [HttpPut("pgs/currentvideo/position/{position}")]
        public async Task<IActionResult> SetPgsVideoPosition(double position, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsVideoPosition(new StateViewModel() { Value = position.ToString() });
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

        [HttpPut("pgs/state/{state}")]
        public async Task<IActionResult> SetPgsPlaybackState(bool state, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetPgsState(new StateViewModel() { Value = state.ToString() });
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

        #region Timeout

        [HttpPut("timeout/pages/next")]
        public async Task<IActionResult> NextPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.NextPage();
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

        [HttpPut("timeout/pages/previous")]
        public async Task<IActionResult> PreviousPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.PreviousPage();
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

        [HttpPut("timeout/pages/{pageNumber}")]
        public async Task<IActionResult> SetPage(string pageNumber, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _pgsTimeoutManager.SetTimeoutPage(new StateViewModel() { Value = pageNumber.ToString() });
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

        [HttpGet("timeout/pages/current")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetCurrentPage([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetTimeoutPage();
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

        [HttpGet("timeout/pages/count")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetPagesCount([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetTimeoutPageCount();
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

        [HttpGet("timeout/file/path")]
        [Produces(typeof(StateViewModel))]
        public async Task<IActionResult> GetFilePath([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _pgsTimeoutManager.GetTimeoutPdfPath();
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
