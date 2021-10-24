using System;
using System.Threading.Tasks;
using Avalanche.Api.Helpers;
using Avalanche.Shared.Infrastructure.Configuration;
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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FeaturesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFeatureManager _featureManager;
        private readonly IWebHostEnvironment _environment;

        public FeaturesController(IFeatureManager featureManager,
            ILogger<MaintenanceController> logger,
            IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _featureManager = featureManager;
        }

        [HttpGet("")]
        [Produces(typeof(FeaturesConfiguration))]
        public async Task<IActionResult> GetFeatures()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = FeaturesHelper.GetFeatures(_featureManager).Result;
                return Ok(result);
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
