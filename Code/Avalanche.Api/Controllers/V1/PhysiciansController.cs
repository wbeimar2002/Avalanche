using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class PhysiciansController : ControllerBase
    {
        readonly IAppLoggerService _appLoggerService;

        public PhysiciansController(IAppLoggerService appLoggerService)
        {
            _appLoggerService = appLoggerService;
        }

        [HttpGet("")]
        [Produces(typeof(List<Physician>))]
        public async Task<IActionResult> GetAll([FromServices]IWebHostEnvironment env)
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
        [Produces(typeof(Physician))]
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

        [HttpPost("")]
        [Produces(typeof(Physician))]
        public async Task<IActionResult> Post(Physician newPhysician, [FromServices]IWebHostEnvironment env)
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

        [HttpPut("{id}")]
        [Produces(typeof(Physician))]
        public async Task<IActionResult> Put(string id, Physician existingPhysician, [FromServices]IWebHostEnvironment env)
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

        [HttpDelete("{id}")]
        [Produces(typeof(Physician))]
        public async Task<IActionResult> Put(string id, [FromServices]IWebHostEnvironment env)
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