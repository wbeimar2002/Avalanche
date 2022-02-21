using System;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMaintenanceManager _maintenanceManager;
        private readonly IWebHostEnvironment _environment;

        public MaintenanceController(IMaintenanceManager maintenanceManager,
            ILogger<MaintenanceController> logger,
            IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }

        /// <summary>
        /// Update policies of a maintenance page and the json values if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        [HttpPut("categories/{key}/policies")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveCategoryPolicies(string key, [FromBody] DynamicSectionViewModel section)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveCategoryPolicies(section);

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
        /// Save json values of a maintenance page.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        [HttpPut("categories/{key}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveCategory(string key, [FromBody]DynamicSectionViewModel section)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveCategory(section);

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
        /// Add a new item to a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        [HttpPost("categories/lists/{key}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddEntity(string key, [FromBody] DynamicListViewModel list)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Insert);

                return Ok();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);

                if (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
                    return Conflict(ex.Get(_environment.IsDevelopment()));
                else
                    return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
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
        /// Update an item from a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        [HttpPut("categories/lists/{key}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateEntity(string key, [FromBody] DynamicListViewModel list)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Update);

                return Ok();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);

                if (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
                    return Conflict(ex.Get(_environment.IsDevelopment()));
                else
                    return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));

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
        /// Delete an item from a list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        [HttpDelete("categories/lists/{key}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteEntity(string key, [FromBody] DynamicListViewModel list)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _maintenanceManager.SaveEntityChanges(list, DynamicListActions.Delete);

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
        /// Get metadata and values for a Maintenance Page
        /// </summary>
        /// <param name="key"></param>
        [HttpGet("categories/{key}")]
        [Produces(typeof(DynamicSectionViewModel))]
        public async Task<IActionResult> GetCategoryByKey(string key)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _maintenanceManager.GetCategoryByKey(key);

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
        /// Get metadata and values for a List Maintenance Page
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parentId"></param>
        [HttpGet("categories/lists/{key}/{parentId}")]
        [Produces(typeof(DynamicListViewModel))]
        public async Task<IActionResult> GetCategoryListByKey(string key, string parentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _maintenanceManager.GetCategoryListByKey(key, parentId);

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
