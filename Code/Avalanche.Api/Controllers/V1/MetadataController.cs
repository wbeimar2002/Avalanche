using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.ViewModels;
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
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IMetadataManager _metadataManager;

        public MetadataController(ILogger<MetadataController> appLoggerService, IMetadataManager metadataManager)
        {
            _appLoggerService = appLoggerService;
            _metadataManager = metadataManager;
        }

        [EnableCors]
        [HttpGet("genders")]
        [Produces(typeof(List<KeyValuePairViewModel>))]
        public async Task<IActionResult> GetGenders([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _metadataManager.GetMetadata(Shared.Domain.Enumerations.MetadataTypes.Genders);
                return Ok(result);
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
        [Produces(typeof(List<KeyValuePairViewModel>))]
        public async Task<IActionResult> GetProcedureTypes([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = _metadataManager.GetMetadata(Shared.Domain.Enumerations.MetadataTypes.ProcedureTypes);
                return Ok(result);
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