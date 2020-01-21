using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Services.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProceduresController : ControllerBase
    {
        readonly IAppLoggerService _appLoggerService;

        public ProceduresController(IAppLoggerService appLoggerService)
        {
            _appLoggerService = appLoggerService;
        }

        [HttpGet("")]
        [Produces(typeof(List<Procedure>))]
        public async Task<IActionResult> Search(ProcedureSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpGet("{id}")]
        [Produces(typeof(ProcedureDetailsViewModel))]
        public async Task<IActionResult> Get([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}