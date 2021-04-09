using Avalanche.Api.Managers.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
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
        readonly ILogger _logger;
        readonly IRoutingManager _routingManager;
        readonly IWebHostEnvironment _environment;

        public DevicesController(ILogger<DevicesController> logger, IRoutingManager routingManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _routingManager = ThrowIfNullOrReturn(nameof(routingManager), routingManager); ;
        }

        /// <summary>
        /// Enter full screen mode 
        /// </summary>
        /// <param name="routingActionViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("fullscreen")]
        public async Task<IActionResult> EnterFullScreen(RoutingActionViewModel routingActionViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.EnterFullScreen(routingActionViewModel);
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
        /// Exit full screen mode
        /// </summary>
        /// <param name="routingActionViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("fullscreen")]
        public async Task<IActionResult> ExitFullScreen(RoutingActionViewModel routingActionViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.ExitFullScreen(routingActionViewModel);
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
        /// Hide preview 
        /// </summary>
        /// <param name="routingPreviewViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("preview")]
        public async Task<IActionResult> HidePreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.HidePreview(routingPreviewViewModel);
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
        /// Show preview
        /// </summary>
        /// <param name="routingPreviewViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("preview")]
        public async Task<IActionResult> ShowPreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.ShowPreview(routingPreviewViewModel);
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
        /// Add a new route
        /// </summary>
        /// <param name="routesViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("routes")]
        public async Task<IActionResult> RouteVideoSource(RoutesViewModel routesViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.RouteVideoSource(routesViewModel);
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
        /// Delete a route
        /// </summary>
        /// <param name="routesViewModel"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("routes")]
        public async Task<IActionResult> UnrouteVideoSource(RoutesViewModel routesViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.UnrouteVideoSource(routesViewModel);
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
        /// Get operating sources
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("operating/sources")]
        [Produces(typeof(IList<VideoSourceModel>))]
        public async Task<IActionResult> GetRoutingSources([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetRoutingSources();
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

        /// <summary>
        /// Get operating outputs
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("operating/outputs")]
        [Produces(typeof(IList<VideoSinkModel>))]
        public async Task<IActionResult> GetRoutingSinks([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetRoutingSinks();
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

        /// <summary>
        /// Get alternative source for a sink
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="index"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("operating/sources/alternative")]
        [Produces(typeof(VideoSourceModel))]
        public async Task<IActionResult> GetAlternativeSource([FromQuery] string alias, [FromQuery] string index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetAlternativeSource(new SinkModel()
                {
                    Alias = alias,
                    Index = index
                });

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
