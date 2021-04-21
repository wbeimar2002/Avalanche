using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Security;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class FilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISecurityManager _securityManager;
        private readonly IRecordingManager _recordingManager;
        private readonly IWebHostEnvironment _environment;

        public FilesController(ILogger<FilesController> logger, 
            ISecurityManager securityManager, 
            ICookieValidationService cookieValidationService,
            IRecordingManager recordingManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _securityManager = securityManager;
            _recordingManager = recordingManager;
        }

        // NOTE: keeping cookie management on the same controller (route) as file access means we can easily scope both the cookie and authentication scheme to just this controller
        [HttpPost("cookies")]
        [AllowAnonymous]
        public async Task<IActionResult> AcquireFileCookieNew([FromBody] string jwtToken)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var identity = _securityManager.AcquireFileCookie(jwtToken);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

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

        [HttpDelete("cookies")]
        public async Task<IActionResult> RevokeFileCookieNew()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                if (_securityManager.RevokeFileCookie())
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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

#warning TODO: This is wrong and intended only for a workflow demo. Replace.
        // TODO: Need to define and implement correct image retrieval patterns. Not in scope of current work.  
        //      - Library ID needs to come with request, so we can determine the correct root path.
        //      - Need some sort of "local" vs "vss" status so we know if we need to proxy the request to the vss.
        // NOTE: A separate endpoint is probably best for video files as well, since those need to support range headers / chunking
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 24)]
        [HttpGet("captures/preview")]
        public IActionResult GetCapturesPreview([FromQuery]string path)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCapturePreview(path);
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
        public IActionResult GetCapturesVideo([FromQuery] string path)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCaptureVideo(path);
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