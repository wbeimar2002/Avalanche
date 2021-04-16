using Avalanche.Api.Managers.Media;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Recorder.Core.V1.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class RecordingController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRecordingManager _recordingManager;
        private readonly IWebHostEnvironment _environment;

        public RecordingController(ILogger<LicensesController> logger, IRecordingManager recordingManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _recordingManager = recordingManager;
        }

        /// <summary>
        /// Start recording
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<IActionResult> StartRecording([FromServices] IWebHostEnvironment env)
        {
            try
            {
                
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _recordingManager.StartRecording();
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
        /// Stop recording
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("")]
        public async Task<IActionResult> StopRecording([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _recordingManager.StopRecording();
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
        /// Add a capture
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("captures")]
        public async Task<IActionResult> CaptureImages([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _recordingManager.CaptureImage();
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
