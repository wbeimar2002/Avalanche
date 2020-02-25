using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LicensesController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly ILicensingManager _licensingManager;

        public LicensesController(ILogger<LicensesController> appLoggerService, ILicensingManager licensingManager)
        {
            _appLoggerService = appLoggerService;
            _licensingManager = licensingManager;
        }

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

        [HttpGet("")]
        [Produces(typeof(List<License>))]
        public async Task<IActionResult> GetAllActive([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                List<License> result = await _licensingManager.GetAllActive();
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