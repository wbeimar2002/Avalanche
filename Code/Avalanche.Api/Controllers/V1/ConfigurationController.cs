using System;
using System.Collections.Generic;
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
    public class ConfigurationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _maintenanceManager;
        private readonly IWebHostEnvironment _environment;

        public ConfigurationController(IConfigurationManager maintenanceManager, ILogger<ConfigurationController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }

        [HttpPut("AutoLabelsConfiguration/{procedureTypeId}")]
        public async Task<IActionResult> UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, [FromBody]List<AutoLabelAutoLabelsConfiguration> autoLabels)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.UpdateAutoLabelsConfigurationByProcedureType(procedureTypeId, autoLabels);
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

        [HttpGet("AutoLabelsConfiguration/{procedureTypeId}")]
        [Produces(typeof(AutoLabelsConfiguration))]
        public async Task<IActionResult> GetAutoLabelsConfigurationSettingsByProcedureType(int procedureTypeId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetAutoLabelsConfigurationSettings(procedureTypeId);
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

        [HttpGet("LabelsConfiguration")]
        [Produces(typeof(LabelsConfiguration))]
        public async Task<IActionResult> GetLabelsConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetLabelsConfigurationSettings();
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

        [HttpGet("SetupConfiguration")]
        [Produces(typeof(SetupConfiguration))]
        public async Task<IActionResult> GetSetupConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetSetupConfigurationSettings();
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

        [HttpGet("RecorderConfiguration")]
        [Produces(typeof(RecorderConfiguration))]
        public async Task<IActionResult> GetRecorderConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetRecorderConfigurationSettings();
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

        [HttpGet("ProceduresSearchConfiguration")]
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
    }
}
