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
        readonly ILogger _appLoggerService;
        readonly ILicensingManager _licensingManager;

        public LicensesController(ILogger<LicensesController> appLoggerService, ILicensingManager licensingManager)
        {
            _appLoggerService = appLoggerService;
            _licensingManager = licensingManager;
        }

        /// <summary>
        /// Validate license key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("{key}")]
        public async Task<IActionResult> Validate(string key, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _licensingManager.Validate(key);
                return Ok();
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
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                List<LicenseModel> result = await _licensingManager.GetAllActive();
                return Ok(result);
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