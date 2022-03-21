using System;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Maintenance;
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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SharedConfigurationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISharedConfigurationManager _maintenanceManager;
        private readonly IWebHostEnvironment _environment;

        public SharedConfigurationController(ISharedConfigurationManager maintenanceManager, ILogger<SharedConfigurationController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }

        [HttpGet("settings/PrintingConfiguration")]
        [Produces(typeof(PrintingConfiguration))]
        public async Task<IActionResult> GetPrintingConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetPrintingConfigurationSettings();
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
