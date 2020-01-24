﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Settings;
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
    public class SettingsController : ControllerBase
    {
        readonly IAppLoggerService _appLoggerService;
        readonly ISettingsManager _settingsManager;

        public SettingsController(IAppLoggerService appLoggerService,
            ISettingsManager settingsManager)
        {
            _appLoggerService = appLoggerService;
            _settingsManager = settingsManager;
        }

        [HttpGet("categories")]
        [Produces(typeof(List<SettingCategory>))]
        public async Task<IActionResult> GetCategories([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                return Ok(await _settingsManager.GetCategories());
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

        [HttpGet("categories/{key}")]
        [Produces(typeof(SettingCategoryViewModel))]
        public async Task<IActionResult> GetSettingsByCategory(string key, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                return Ok(await _settingsManager.GetSettingsByCategory(key));
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

        [HttpPost("categories/{categoryKey}")]
        public async Task<IActionResult> SaveSettingsByCategory([FromServices]IWebHostEnvironment env)
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

        [HttpGet("categories/{categoryKey}/sources/{sourcekey}")]
        [Produces(typeof(SettingCategoryViewModel))]
        public async Task<IActionResult> GetSettingsByCategory(string categoryKey, string sourcekey, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                return Ok(_settingsManager.GetSourceValuesByCategory(categoryKey, sourcekey));
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