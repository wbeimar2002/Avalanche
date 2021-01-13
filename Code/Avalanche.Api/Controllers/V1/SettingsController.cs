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
using Newtonsoft.Json;
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

        [HttpGet("{key}")]
        public async Task<IActionResult> GetSettings(string key, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _maintenanceManager.GetSettingValues(key + "Values",User.GetUser());
                return Content(JsonConvert.SerializeObject(response), "application/json");
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