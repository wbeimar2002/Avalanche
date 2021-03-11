using Avalanche.Api.Managers.Security;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Infrastructure.Constants;
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
        readonly ILogger _appLoggerService;
        private ISecurityManager _securityManager;
        private ICookieValidationService _cookieValidationService;

        public FilesController(ILogger<FilesController> appLoggerService, ISecurityManager securityManager, ICookieValidationService cookieValidationService)
        {
            _appLoggerService = appLoggerService;
            _securityManager = securityManager;
            _cookieValidationService = cookieValidationService;
        }

        // NOTE: keeping cookie management on the same controller (route) as file access means we can easily scope both the cookie and authentication scheme to just this controller
        [HttpPost("acquireFileCookie")]
        [AllowAnonymous]
        public async Task<IActionResult> AcquireFileCookie([FromServices]IWebHostEnvironment env, [FromBody] string jwtToken)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var identity = _securityManager.CreateTokenIdentity(jwtToken, CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(AvalancheClaimTypes.LastChanged, DateTimeOffset.Now.ToString()));
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpPost("revokeFileCookie")]
        public async Task<IActionResult> RevokeFileCookie([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var user = HttpContext.User;
                if (user?.Identity?.IsAuthenticated ?? false)
                {
                    _cookieValidationService.RevokePrincipal(user);
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(env.IsDevelopment()));
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
        public IActionResult DemoGetImageFile([FromQuery]string path)
        {
            try
            {
                var libraryRoot = Environment.GetEnvironmentVariable("demoLibraryFolder");
                var translated = path.Replace('\\', '/').TrimStart('/');
                var fullPath = System.IO.Path.Combine(libraryRoot, translated);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
        }
    }
}