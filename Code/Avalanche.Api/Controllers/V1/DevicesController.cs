using Avalanche.Api.Extensions;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Managers.PgsTimeout;
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
using System.Threading.Tasks;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        readonly ILogger _appLoggerService;

        readonly IDevicesManager _devicesManager;

        private readonly IPgsTimeoutManager _pgsTimeoutManager;

        public DevicesController(IDevicesManager devicesManager, IPgsTimeoutManager pgsTimeoutManager, ILogger<DevicesController> logger)
        {
            _appLoggerService = logger;
            _devicesManager = devicesManager;
            _pgsTimeoutManager = ThrowIfNullOrReturn(nameof(pgsTimeoutManager), pgsTimeoutManager);
        }

        /// <summary>
        /// Send a command to the output
        /// </summary>
        [HttpPut("commands")]
        [Produces(typeof(List<CommandResponse>))]
        public async Task<IActionResult> SendCommand([FromBody]CommandViewModel command, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _devicesManager.SendCommand(command);

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

        /// <summary>
        /// Returns all the destinations for operations 
        /// </summary>
        [HttpGet("outputs/operations")]
        [Produces(typeof(List<VideoSink>))]
        public async Task<IActionResult> GetOperationsOuputs([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _devicesManager.GetRoutingSinks();
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

        /// <summary>
        /// Returns all the destinations for operations 
        /// </summary>
        [HttpGet("sources/operations")]
        [Produces(typeof(List<VideoSource>))]
        public async Task<IActionResult> GetOperationsSources([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _devicesManager.GetRoutingSources();
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

        /// <summary>
        /// Gets an alternative source. Used for dynamic sources
        /// Call this as a response to a source identity changed event
        /// </summary>
        [HttpGet("sources/{alias}:{index}/alternativeSource")]
        [Produces(typeof(VideoSource))]
        public async Task<IActionResult> GetAlternativeSource(string alias, int index, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _devicesManager.GetAlternativeSource(alias, index);
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