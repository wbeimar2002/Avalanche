using Avalanche.Api.Extensions;
using Avalanche.Api.Managers.Settings;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class SettingsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly ISettingsManager _settingsManager;

        public SettingsController(ISettingsManager settingsManager, ILogger<SettingsController> logger)
        {
            _appLoggerService = logger;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Return the video routing settings
        /// </summary>
        [HttpGet("videorouting")]
        [Produces(typeof(VideoRoutingSettings))]
        public async Task<IActionResult> GetVideoRoutingSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var response = await _settingsManager.GetVideoRoutingSettingsAsync(User.GetUser());
                return Ok(response);
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
        /// Return the setup settings
        /// </summary>
        [HttpGet("setup")]
        [Produces(typeof(SetupSettings))]
        public async Task<IActionResult> GetSetupSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var response = await _settingsManager.GetSetupSettingsAsync(User.GetUser());
                return Ok(response);
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
        /// Return the timeout settings
        /// </summary>
        [HttpGet("timeout")]
        [Produces(typeof(TimeoutSettings))]
        public async Task<IActionResult> GetTimeoutSettings([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var response = await _settingsManager.GetTimeoutSettingsAsync();
                return Ok(response);
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
        /// Get categories
        /// </summary>
        [HttpGet("categories")]
        [Produces(typeof(List<SettingCategory>))]
        public async Task<IActionResult> GetCategories([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                return Ok(await _settingsManager.GetCategories());
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
        /// Get category using key
        /// </summary>
        [HttpGet("categories/{key}")]
        [Produces(typeof(SettingCategoryViewModel))]
        public async Task<IActionResult> GetSettingsByCategory(string key, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                return Ok(await _settingsManager.GetSettingsByCategory(key));
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
        /// Save values from a category
        /// </summary>
        [HttpPost("categories/{categoryKey}")]
        public async Task<IActionResult> SaveSettingsByCategory(string categoryKey, [FromBody]List<KeyValuePairViewModel> settings, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _settingsManager.SaveSettingsByCategory(categoryKey, settings);
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
        /// Get sources from a category and a source key
        /// </summary>
        [HttpGet("categories/{categoryKey}/sources/{sourcekey}")]
        [Produces(typeof(SettingCategoryViewModel))]
        public async Task<IActionResult> GetSettingsByCategory(string categoryKey, string sourcekey, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
                return Ok(_settingsManager.GetSourceValuesByCategory(categoryKey, sourcekey));
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