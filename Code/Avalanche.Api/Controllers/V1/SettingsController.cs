using Avalanche.Api.Extensions;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IMaintenanceManager _maintenanceManager;

        public SettingsController(IMaintenanceManager maintenanceManager, ILogger<FilesController> appLoggerService)
        {
            _appLoggerService = appLoggerService;
            _maintenanceManager = maintenanceManager;
        }

        /// <summary>
        /// Return the PGS settings
        /// </summary>
        [HttpGet("pgs")]
        [Produces(typeof(PgsSettings))]
        public async Task<IActionResult> GetPgsSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _maintenanceManager.GetSettings<PgsSettings>("PgsSettings",User.GetUser());
                return Ok(response);
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
        /// Return the timeout settings
        /// </summary>
        [HttpGet("timeout")]
        [Produces(typeof(TimeoutSettings))]
        public async Task<IActionResult> GetTimeoutSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _maintenanceManager.GetSettings<TimeoutSettings>("TimeoutSettings", User.GetUser());
                return Ok(response);
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
        /// Return the setup settings
        /// </summary>
        [HttpGet("setup")]
        [Produces(typeof(SetupSettings))]
        public async Task<IActionResult> GetSetupSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _maintenanceManager.GetSettings<SetupSettings>("SetupSettings", User.GetUser());
                return Ok(response);
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
        /// Return the timeout settings
        /// </summary>
        [HttpGet("surgery")]
        [Produces(typeof(SurgerySettings))]
        public async Task<IActionResult> GetSurgerySettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _maintenanceManager.GetSettings<SurgerySettings>("SurgerySettings", User.GetUser());
                return Ok(response);
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