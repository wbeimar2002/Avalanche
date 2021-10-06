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
    public class DeviceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRoutingManager _routingManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;

        public DeviceController(ILogger<DeviceController> logger, IRoutingManager routingManager, IMapper mapper, IWebHostEnvironment environment)
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
        [HttpPost("videorouting/mode/fullscreen")]
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
        /// Hide preview 
        /// </summary>
        /// <param name="routingPreviewViewModel"></param>
        /// <returns></returns>
        [HttpDelete("hardware/preview")]
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
        [HttpPost("hardware/preview")]
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
        [HttpPost("videorouting/routes")]
        public async Task<IActionResult> RouteVideoSource([FromBody] RouteViewModel routesViewModel)
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
        [HttpDelete("videorouting/routes")]
        public async Task<IActionResult> UnrouteVideo([FromQuery] AliasIndexViewModel sink)
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
        [HttpGet("videorouting/sources")]
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
        [HttpGet("videorouting/sinks")]
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
        [HttpGet("videorouting/sources/alternative")]
        [Produces(typeof(VideoSourceModel))]
        public async Task<IActionResult> GetAlternativeSource([FromQuery] AliasIndexViewModel source)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetAlternativeSource(new AliasIndexModel()
                {
                    Alias = source.Alias,
                    Index = source.Index
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

        [HttpPut("videorouting/displayRecording/status")]
        public async Task<IActionResult> SetDisplayRecordingStatus([FromBody] DisplayRecordingRequestViewModel displayRecordingRequestModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.SetDisplayRecordingStatus(displayRecordingRequestModel);

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

        [HttpGet("videorouting/displayRecording/statuses")]
        [Produces(typeof(IList<DisplayRecordingViewModel>))]
        public async Task<IActionResult> GetDisplayRecordingStatuses()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _routingManager.GetDisplayRecordingStatuses();

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

        [HttpPut("videorouting/sources/selected")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetSelectedSource([FromBody] AliasIndexViewModel selectedSource)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _routingManager.SetSelectedSource(new AliasIndexModel
                {
                    Alias = selectedSource.Alias,
                    Index = selectedSource.Index
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

        [HttpGet("videorouting/sources/selected")]
        [ProducesResponseType(typeof(AliasIndexViewModel), StatusCodes.Status200OK)]
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