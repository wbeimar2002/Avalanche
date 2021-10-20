using Avalanche.Api.Helpers;
using Avalanche.Api.ViewModels;
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
using System.Threading.Tasks;

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
        [Produces(typeof(HealthCheckViewModel))]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _logger.LogInformation("Avalanche Api is healthy.");
                return new OkObjectResult(new HealthCheckViewModel
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime(),
                    Features = await FeaturesHelper.GetFeatures(_featureManager)
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
        [Produces(typeof(HealthCheckViewModel))]
        public async Task<IActionResult> HealthCheckSecure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _logger.LogInformation("Avalanche Api is healthy.");

                return new OkObjectResult(new HealthCheckViewModel
                {
                    UtcDateTime = DateTime.UtcNow,
                    LocalDateTime = DateTime.UtcNow.ToLocalTime(),
                    Features = await FeaturesHelper.GetFeatures(_featureManager)
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
    }
}
