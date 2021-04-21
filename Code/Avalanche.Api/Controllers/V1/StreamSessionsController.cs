using Avalanche.Api.Managers.Media;
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
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StreamSessionsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWebRTCManager _webRTCManager;
        private readonly IWebHostEnvironment _environment;

        public StreamSessionsController(ILogger<LicensesController> logger, IWebRTCManager webRTCManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _webRTCManager = webRTCManager;
        }

        /// <summary>
        /// Get source strems for WebRTC
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("sources")]
        [Produces(typeof(IList<VideoDeviceModel>))]
        public async Task<IActionResult> GetSourceStreams([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _webRTCManager.GetSourceStreams();
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
        /// Play video with WebRTC
        /// </summary>
        /// <param name="session"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces(typeof(List<string>))]
        public async Task<IActionResult> InitSession(WebRTCSessionModel session)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _webRTCManager.InitSessionAsync(session);
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
        /// Handle WebRTC message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("")]
        public async Task<IActionResult> HandleMessage(WebRTCMessaggeModel message)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _webRTCManager.HandleMessageForVideo(message);
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
        /// Stop WebRTC video reproduction
        /// </summary>
        /// <param name="message"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("")]
        public async Task<IActionResult> DeInitSession(WebRTCMessaggeModel message)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _webRTCManager.DeInitSessionAsync(message);
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
    }
}
