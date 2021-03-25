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
        private readonly ILogger _appLoggerService;
        private readonly ISecurityManager _securityManager;
        private readonly IRecordingManager _recordingManager;


        public FilesController(ILogger<FilesController> appLoggerService, 
            ISecurityManager securityManager, 
            ICookieValidationService cookieValidationService,
            IRecordingManager recordingManager)
        {
            _appLoggerService = appLoggerService;
            _securityManager = securityManager;
            _recordingManager = recordingManager;
        }

        // NOTE: keeping cookie management on the same controller (route) as file access means we can easily scope both the cookie and authentication scheme to just this controller
        [HttpPost("cookies")]
        [AllowAnonymous]
        public async Task<IActionResult> AcquireFileCookieNew([FromServices] IWebHostEnvironment env, [FromBody] string jwtToken)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var identity = _securityManager.AcquireFileCookie(jwtToken);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Ok();
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpDelete("cookies")]
        public async Task<IActionResult> RevokeFileCookieNew([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                if (_securityManager.RevokeFileCookie())
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Ok();
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
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
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _recordingManager.CaptureImage();
                return Ok();
            }
            catch (Grpc.Core.RpcException ex)
            {
                _appLoggerService.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest(ex.Get(Request.Path.ToString(), env.IsDevelopment()));
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpGet("captures/preview")]
        [AllowAnonymous]
        public IActionResult GetCapturesPreview([FromQuery] string path, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var fullPath = _recordingManager.GetCapturePreview(path);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Grpc.Core.RpcException ex)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest(ex.Get(Request.Path.ToString(), env.IsDevelopment()));
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

#warning TODO: This is wrong and intended only for a workflow demo. Replace.
        // TODO: Need to define and implement correct image retrieval patterns. Not in scope of current work.  
        //      - Library ID needs to come with request, so we can determine the correct root path.
        //      - Need some sort of "local" vs "vss" status so we know if we need to proxy the request to the vss.
        // NOTE: A separate endpoint is probably best for video files as well, since those need to support range headers / chunking
        [HttpGet("DemoGetImageFile")]
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 24)]
        public IActionResult DemoGetImageFile([FromQuery] string path)
        {
            try
            {
                var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
                var translated = path.Replace('\\', '/').TrimStart('/');
                var fullPath = System.IO.Path.Combine(libraryRoot, translated);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return BadRequest();
            }
        }
    }
}