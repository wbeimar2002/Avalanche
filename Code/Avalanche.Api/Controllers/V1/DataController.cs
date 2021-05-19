using Avalanche.Api.Managers.Data;
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
    public class DataController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDataManager _metadataManager;
        private readonly IWebHostEnvironment _environment;

        public DataController(ILogger<DataController> logger, IDataManager metadataManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _metadataManager = ThrowIfNullOrReturn(nameof(metadataManager), metadataManager);
        }

        /// <summary>
        /// Get search columns configuration
        /// </summary>
        /// <returns></returns>
        [HttpGet("searchColumns")]
        [Produces(typeof(List<DynamicSourceKeyValuePairViewModel>))]
        public async Task<IActionResult> GetSearchColumns()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetSource(DataTypes.SearchColumns);
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
        /// Get sexes list
        /// </summary>
        /// <returns></returns>
        [HttpGet("sexes")]
        [Produces(typeof(List<KeyValuePairViewModel>))]
        public async Task<IActionResult> GetSexes()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetData(DataTypes.Sex);
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
        /// <returns></returns>
        [HttpGet("departments")]
        [Produces(typeof(List<DepartmentModel>))]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetAllDepartments();
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
        /// Add new department
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        [HttpPost("departments")]
        [Produces(typeof(DepartmentModel))]
        public async Task<IActionResult> AddDepartment(DepartmentModel department)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.AddDepartment(department);
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
        /// Delete department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        [HttpDelete("departments/{departmentId}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteDepartment(departmentId);
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
        /// Get departments by procedure type
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        [HttpGet("departments/{departmentId}/procedureTypes")]
        [Produces(typeof(List<ProcedureTypeModel>))]
        public async Task<IActionResult> GetProcedureTypesByDepartment(int departmentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetProcedureTypesByDepartment(departmentId);
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
        /// Get procedure types with null department
        /// </summary>
        /// <returns></returns>
        [HttpGet("procedureTypes")]
        [Produces(typeof(List<ProcedureTypeModel>))]
        public async Task<IActionResult> GetProcedureTypes()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetProcedureTypesByDepartment(null);
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
        /// Add a procedure type
        /// </summary>
        /// <param name="procedureType"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("procedureTypes")]
        [Produces(typeof(ProcedureTypeModel))]
        public async Task<IActionResult> AddProcedureType(ProcedureTypeModel procedureType)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.AddProcedureType(procedureType);
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
        /// Delete a procedure type
        /// </summary>
        /// <param name="procedureTypeName"></param>
        /// <returns></returns>
        [HttpDelete("procedureTypes/{procedureTypeId}")]
        public async Task<IActionResult> DeleteProcedureType(string procedureTypeName)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteProcedureType(new ProcedureTypeModel()
                {
                    Name = procedureTypeName
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

        /// <summary>
        /// Delete a procedure types from a department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="procedureTypeId"></param>
        /// <returns></returns>
        [HttpDelete("departments/{departmentId}/procedureTypes/{procedureTypeId}")]
        public async Task<IActionResult> DeleteProcedureType(int departmentId, int procedureTypeId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteProcedureType(new ProcedureTypeModel()
                {
                    DepartmentId = departmentId,
                    Id = procedureTypeId
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
    }
}