using AutoMapper;
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
using Microsoft.FeatureManagement.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(FeatureFlags.Devices)]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRoutingManager _routingManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;

        public DevicesController(ILogger<DevicesController> logger, IRoutingManager routingManager, IMapper mapper, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _routingManager = ThrowIfNullOrReturn(nameof(routingManager), routingManager);
        }

        /// <summary>
        /// Enter full screen mode 
        /// </summary>
        /// <param name="routingActionViewModel"></param>
        /// <returns></returns>
        [HttpPost("fullscreen")]
        public async Task<IActionResult> EnterFullScreen(FullScreenRequestViewModel routingActionViewModel)
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
        /// <returns></returns>
        [HttpDelete("fullscreen")]
        public async Task<IActionResult> ExitFullScreen(FullScreenRequestViewModel routingActionViewModel)
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
        /// <returns></returns>
        [HttpPut("routes")]
        public async Task<IActionResult> RouteVideoSource(RouteViewModel routesViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.RouteVideoSource(_mapper.Map<RouteViewModel, RouteModel>(routesViewModel));
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
        /// <param name="sink"></param>
        /// <returns></returns>
        [HttpDelete("routes")]
        public async Task<IActionResult> UnrouteVideo(AliasIndexViewModel sink)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                await _routingManager.UnrouteVideoSource(_mapper.Map<AliasIndexViewModel, AliasIndexModel>(sink));
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
        /// <returns></returns>
        [HttpGet("operating/sources")]
        [Produces(typeof(IList<VideoSourceModel>))]
        public async Task<IActionResult> GetRoutingSources()
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
        /// <returns></returns>
        [HttpGet("operating/outputs")]
        [Produces(typeof(IList<VideoSinkModel>))]
        public async Task<IActionResult> GetRoutingSinks()
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
        /// <returns></returns>
        [HttpGet("operating/sources/alternative")]
        [Produces(typeof(VideoSourceModel))]
        public async Task<IActionResult> GetAlternativeSource([FromQuery] string alias, [FromQuery] string index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetAlternativeSource(new AliasIndexModel()
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

        [HttpPut("displayRecording")]
        public async Task<IActionResult> SetDisplayRecordingEnabled([FromBody] DisplayRecordingRequestViewModel displayRecordingRequestModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.SetDisplayRecordingEnabled(displayRecordingRequestModel);

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

        [HttpGet("displayRecordings")]
        [Produces(typeof(IList<DisplayRecordingViewModel>))]
        public async Task<IActionResult> GetDisplayRecordingStates()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetDisplayRecordingStates();

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

        [HttpPut("operating/sources/setselectedsource")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetSelectedSource([FromQuery] string alias, [FromQuery] string index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.SetSelectedSource(new AliasIndexModel
                {
                    Alias = alias,
                    Index = index
                });

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

        [HttpPut("operating/sources/selected")]
        [ProducesResponseType(typeof(AliasIndexModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSelectedSource()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetSelectedSource();
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
