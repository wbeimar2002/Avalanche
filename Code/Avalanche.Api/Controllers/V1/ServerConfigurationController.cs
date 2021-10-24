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
    public class ServerConfigurationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IServerConfigurationManager _maintenanceManager;
        private readonly IWebHostEnvironment _environment;

        public ServerConfigurationController(IServerConfigurationManager maintenanceManager, ILogger<ServerConfigurationController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }

        [HttpGet("settings/ProceduresSearchConfiguration")]
        [Produces(typeof(ProceduresSearchConfiguration))]
        public async Task<IActionResult> GetProceduresSearchConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetProceduresSearchConfigurationSettings();
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

        [HttpGet("settings/VaultStreamServerConfiguration")]
        [Produces(typeof(VaultStreamServerConfiguration))]
        public async Task<IActionResult> GetVaultStreamServerConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetVaultStreamServerConfigurationSettings();
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
