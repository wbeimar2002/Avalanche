﻿using Avalanche.Api.Managers.Media;
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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StreamSessionsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IWebRTCManager _webRTCManager;

        public StreamSessionsController(ILogger<LicensesController> appLoggerService, IWebRTCManager webRTCManager)
        {
            _appLoggerService = appLoggerService;
            _webRTCManager = webRTCManager;
        }

        /// <summary>
        /// Get source strems for WebRTC
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("sources")]
        [Produces(typeof(IList<VideoDeviceModel>))]
        public async Task<IActionResult> GetSourceStreams([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _webRTCManager.GetSourceStreams();
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
        /// Play video with WebRTC
        /// </summary>
        /// <param name="session"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces(typeof(List<string>))]
        public async Task<IActionResult> InitSession(WebRTCSessionModel session, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _webRTCManager.InitSessionAsync(session);
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
        /// Handle WebRTC message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("")]
        public async Task<IActionResult> HandleMessage(WebRTCMessaggeModel message, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _webRTCManager.HandleMessageForVideo(message);
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

        /// <summary>
        /// Stop WebRTC video reproduction
        /// </summary>
        /// <param name="message"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("")]
        public async Task<IActionResult> DeInitSession(WebRTCMessaggeModel message, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _webRTCManager.DeInitSessionAsync(message);
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
    }
}