using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.Services.Health;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Google.Protobuf.WellKnownTypes;
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
        private readonly IPieService _pieService;

        public ConfigurationController(IConfigurationManager maintenanceManager, ILogger<ConfigurationController> logger, IWebHostEnvironment environment, IPieService pieService)
        {
            _environment = environment;
            _pieService = pieService;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }

        [HttpPut("settings/AutoLabelsConfiguration/{procedureTypeId}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, [FromBody] List<AutoLabelAutoLabelsConfiguration> autoLabels)
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

        #region Settings
        [HttpGet("settings/ProceduresConfiguration")]
        [Produces(typeof(ProceduresConfiguration))]
        public async Task<IActionResult> GetProceduresConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetProceduresConfigurationSettings();
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

        [HttpGet("settings/AutoLabelsConfiguration/{procedureTypeId}")]
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

        [HttpGet("settings/LabelsConfiguration")]
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

        [HttpGet("settings/SetupConfiguration")]
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

        [HttpGet("settings/RecorderConfiguration")]
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

        [HttpGet("settings/MedPresenceConfiguration")]
        [Produces(typeof(MedPresenceConfiguration))]
        public async Task<IActionResult> GetMedPresenceConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetMedPresenceConfigurationSettings();
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

        [HttpGet("settings/FinishOptionsConfiguration")]
        [Produces(typeof(FinishOptionsConfiguration))]
        public async Task<IActionResult> GetFinishOptionsConfigurationSettings()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _maintenanceManager.GetFinishOptionsConfigurationSettings();
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

        /// <summary>
        /// Return the List Source at result
        /// </summary>
        [HttpGet("listsource")]
        [Produces(typeof(int))]
        public async Task<IActionResult> GetListSource()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _pieService.GetPatientListSource(new Empty()).ConfigureAwait(false);
                return Ok(result.Source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
        #endregion Settings
    }
}
