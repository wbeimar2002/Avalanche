using Avalanche.Api.Extensions;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Maintenance;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PhysiciansController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMaintenanceManager _maintenangeManager;
        private readonly IWebHostEnvironment _environment;

        public PhysiciansController(ILogger<PhysiciansController> logger, IMaintenanceManager maintenanceManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenangeManager = maintenanceManager;
        }

        /// <summary>
        /// Get all physicians
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> GetAllPhysicians()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _maintenangeManager.GetListValues("Physicians");
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