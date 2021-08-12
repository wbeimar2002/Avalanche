using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IFeatureManager _featureManager;

        public HealthController(ILogger<HealthController> logger, IWebHostEnvironment environment, IFeatureManager featureManager)
        {
            _featureManager = featureManager;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        ///  Health check without secure
        /// </summary>
        [Route("check")]
        [HttpGet]
        public IActionResult HealthCheck()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _logger.LogInformation("Avalanche Api is healthy.");
                return new OkObjectResult(new
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime(),
                    Features = GetFeatures()
                });
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
        /// Health check with secure
        /// </summary>
        [Authorize]
        [Route("check/secure")]
        [HttpGet]
        public IActionResult HealthCheckSecure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _logger.LogInformation("Avalanche Api is healthy.");

                return new OkObjectResult(new
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime(),
                    Features = GetFeatures()
                });
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

        private object GetFeatures() => new
        {
            ActiveProcedure = _featureManager.IsEnabledAsync(FeatureFlags.ActiveProcedure),
            Devices = _featureManager.IsEnabledAsync(FeatureFlags.Devices),
            Media = _featureManager.IsEnabledAsync(FeatureFlags.Media),
            Presets = _featureManager.IsEnabledAsync(FeatureFlags.Presets),
            Recording = _featureManager.IsEnabledAsync(FeatureFlags.Recording),
            StreamSessions = _featureManager.IsEnabledAsync(FeatureFlags.StreamSessions),
        };
    }
}
