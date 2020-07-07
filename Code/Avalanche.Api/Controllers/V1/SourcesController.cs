﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Devices;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SourcesController : ControllerBase
    {
        readonly ISourcesManager _sourcesManager;
        readonly ILogger _appLoggerService;

        public SourcesController(ISourcesManager sourcesManager, ILogger<SourcesController> appLoggerService)
        {
            _sourcesManager = sourcesManager;
            _appLoggerService = appLoggerService;
        }
        /// <summary>
        /// Returns all the outputs available to reproduce content
        /// </summary>
        [HttpGet("all")]
        [Produces(typeof(List<Source>))]
        public async Task<IActionResult> GetAllAvailable([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _sourcesManager.GetAllAvailable();
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