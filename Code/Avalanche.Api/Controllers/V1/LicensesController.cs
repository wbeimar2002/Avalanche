using Avalanche.Api.Managers.Licensing;
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
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class LicensesController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ILicensingManager _licensingManager;
        readonly IWebHostEnvironment _environment;

        public LicensesController(ILogger<LicensesController> logger, ILicensingManager licensingManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _licensingManager = licensingManager;
        }

        /// <summary>
        /// Validate license key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("{key}")]
        public async Task<IActionResult> Validate(string key)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _licensingManager.Validate(key);
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

        /// <summary>
        /// Get all active software installed
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("")]
        [Produces(typeof(List<LicenseModel>))]
        public async Task<IActionResult> GetAllActive([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                List<LicenseModel> result = await _licensingManager.GetAllActive();
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