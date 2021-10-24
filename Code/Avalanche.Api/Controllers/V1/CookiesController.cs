using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Security;
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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class CookiesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISecurityManager _securityManager;
        private readonly IWebHostEnvironment _environment;

        public CookiesController(ILogger<CookiesController> logger,
            ISecurityManager securityManager,
            IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _securityManager = securityManager;
        }

        // NOTE: keeping cookie management on the same controller (route) as file access means we can easily scope both the cookie and authentication scheme to just this controller
        [HttpPost("")]
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

        [HttpDelete("")]
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
    }
}
