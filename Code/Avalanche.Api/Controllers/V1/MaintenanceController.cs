using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.ViewModels;
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
    public class MaintenanceController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IMaintenanceManager _maintenanceManager;

        public MaintenanceController(IMaintenanceManager maintenanceManager, ILogger<MaintenanceController> appLoggerService)
        {
            _appLoggerService = appLoggerService;
            _maintenanceManager = maintenanceManager;
        }

        /// <summary>
        /// Update policies of a maintenance page and the json values if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("categories/{key}/policies")]
        public async Task<IActionResult> SaveCategoryPolicies(string key, [FromBody] DynamicSectionViewModel section, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveCategoryPolicies(section);

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
        /// Save json values of a maintenance page.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("categories/{key}")]
        public async Task<IActionResult> SaveCategory(string key, [FromBody]DynamicSectionViewModel section,[FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveCategory(section);

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
        /// Add a new item to a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("categories/lists/{key}")]
        public async Task<IActionResult> AddEntity(string key, [FromBody] DynamicListViewModel list, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Insert);

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
        /// Update an item from a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("categories/lists/{key}")]
        public async Task<IActionResult> UpdateEntity(string key, [FromBody] DynamicListViewModel list, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Update);

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
        /// Delete an item from a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("categories/lists/{key}")]
        public async Task<IActionResult> DeleteEntity(string key, [FromBody] DynamicListViewModel list, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Delete);

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
        /// Get metadata and values for a Maintenance Page
        /// </summary>
        /// <param name="key"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("categories/{key}")]
        public async Task<IActionResult> GetCategoryByKey(string key, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _maintenanceManager.GetCategoryByKey(key);

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
        /// Get metadata and values for a List Maintenance Page
        /// </summary>
        /// <param name="key"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("categories/lists/{key}")]
        public async Task<IActionResult> GetCategoryListByKey(string key, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _maintenanceManager.GetCategoryListByKey(key);

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