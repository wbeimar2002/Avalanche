using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IMetadataManager _typesManager;

        public MetadataController(ILogger<MetadataController> appLoggerService, IMetadataManager typesManager)
        {
            _appLoggerService = appLoggerService;
            _typesManager = typesManager;
        }

        [EnableCors]
        [HttpGet("genders")]
        public async Task<IActionResult> GetGenders([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [EnableCors]
        [HttpGet("proceduretypes")]
        public async Task<IActionResult> GetProcedureTypes([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}