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
        /// <returns></returns>
        [HttpPost("")]
        public async Task<IActionResult> StartRecording()
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
        /// <returns></returns>
        [HttpDelete("")]
        public async Task<IActionResult> StopRecording()
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
        /// <returns></returns>
        [HttpPost("captures")]
        public async Task<IActionResult> CaptureImages()
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

        /// <summary>
        /// Gets channels configured for recording
        /// </summary>
        /// <returns></returns>
        [HttpGet("recordChannels")]
        [Produces(typeof(List<RecordingChannelModel>))]
        public async Task<IActionResult> GetRecordingChannels()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var channels = await _recordingManager.GetRecordingChannels();
                return Ok(channels);
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
        /// Gets video id and offset timeline info for the image
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpGet("timeline/video")]
        [Produces(typeof(RecordingTimelineViewModel))]
        public async Task<IActionResult> GetTimelineVideo(Guid imageId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var viewModel = await _recordingManager.GetRecordingTimelineByImageId(imageId);
                if (viewModel == null)
                    return NotFound(imageId);

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}
