using System;
using System.Diagnostics.CodeAnalysis;
using Avalanche.Api.Managers.Media;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class FilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRecordingManager _recordingManager;

        public FilesController(ILogger<CookiesController> logger,
            IRecordingManager recordingManager)
        {
            _logger = logger;
            _recordingManager = recordingManager;
        }

#warning TODO: This is wrong and intended only for a workflow demo. Replace.
        // TODO: Need to define and implement correct image retrieval patterns. Not in scope of current work.  
        //      - Need some sort of "local" vs "vss" status so we know if we need to proxy the request to the vss.
        // NOTE: A separate endpoint is probably best for video files as well, since those need to support range headers / chunking
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 24)]
        [HttpGet("captures/preview")]
        public IActionResult GetCapturesPreview([FromQuery] string path, [FromQuery] string procedureId, [FromQuery] string repository)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCapturePreview(path, procedureId, repository);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

#warning TODO: This is wrong and intended only for a workflow demo. Replace.
        // TODO: Need to define and implement correct image retrieval patterns. Not in scope of current work.  
        //      - Need some sort of "local" vs "vss" status so we know if we need to proxy the request to the vss.
        // NOTE: A separate endpoint is probably best for video files as well, since those need to support range headers / chunking
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 24)]
        [HttpGet("captures/image")]
        public IActionResult GetCapturesImage([FromQuery] string path, [FromQuery] string procedureId, [FromQuery] string repository)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCapturePreview(path, procedureId, repository);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 24)]
        [HttpGet("captures/video")]
        public IActionResult GetCapturesVideo([FromQuery] string path, [FromQuery] string procedureId, [FromQuery] string repository)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCaptureVideo(path, procedureId, repository);
                // for video, add range headers to support chunking / seek (and allow safari to work at all)
                return PhysicalFile(fullPath, "video/mp4", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}
