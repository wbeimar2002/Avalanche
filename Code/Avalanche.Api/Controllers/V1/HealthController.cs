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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        readonly ILogger _appLoggerService;

        public HealthController(ILogger<HealthController> logger)
        {
            _appLoggerService = logger;
        }

        /// <summary>
        /// Health check without secure
        /// </summary>
        [Route("check")]
        [HttpGet]
        public IActionResult HealthCheck([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                _appLoggerService.LogInformation("Avalanche Api is healthy.");
                
                return new OkObjectResult(new
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime()
                });
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

        /// <summary>
        /// Health check with secure
        /// </summary>
        [Authorize]
        [Route("check/secure")]
        [HttpGet]
        public IActionResult HealthCheckSecure([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _appLoggerService.LogInformation("Avalanche Api is healthy.");

                return new OkObjectResult(new
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime()
                });
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
    }
}