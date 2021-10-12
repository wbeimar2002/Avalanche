using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.Helpers;
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

        public MaintenanceController(IMaintenanceManager maintenanceManager, ILogger<MaintenanceController> logger, GeneralApiConfiguration config, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _maintenanceManager = maintenanceManager;
        }


        [HttpGet("{sourceKey}")]
        public async Task<IActionResult> GetList(string sourceKey)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _maintenanceManager.GetList(sourceKey);
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

        [HttpGet("{sourceKey}/{jsonKey}")]
        public async Task<IActionResult> GetList(string sourceKey, string jsonKey)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _maintenanceManager.GetList(sourceKey, jsonKey);
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
        /// Get all departments
        /// </summary>
        [HttpGet("sexes")]
        [Produces(typeof(List<KeyValuePairViewModel>))]
        public async Task<IActionResult> GetSexes()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var list = await _maintenanceManager.GetList("Sexes");
                var result = SerializationHelper.Json(list);
                return Ok(SerializationHelper.Get<List<KeyValuePairViewModel>>(result));
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
        /// Update policies of a maintenance page and the json values if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section"></param>
        [HttpPut("categories/{key}/policies")]
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

        [HttpPut("operations/indexes")]
        public async Task<IActionResult> ReindexRepository([FromBody] ReindexRepositoryRequestViewModel request)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _maintenanceManager.ReindexRepository(request);

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
