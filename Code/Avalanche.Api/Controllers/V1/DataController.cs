﻿using Avalanche.Api.Managers.Data;
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
        readonly ILogger _appLoggerService;
        readonly IDataManager _metadataManager;

        public DataController(ILogger<DataController> appLoggerService, IDataManager metadataManager)
        {
            _appLoggerService = ThrowIfNullOrReturn(nameof(appLoggerService), appLoggerService);
            _metadataManager = ThrowIfNullOrReturn(nameof(metadataManager), metadataManager); ;
        }

        /// <summary>
        /// Get search columns configuration
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("searchColumns")]
        [Produces(typeof(List<DynamicSourceKeyValuePairViewModel>))]
        public async Task<IActionResult> GetSearchColumns([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetSource(DataTypes.SearchColumns);
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
        /// Get sexes list
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("sexes")]
        [Produces(typeof(List<KeyValuePairViewModel>))]
        public async Task<IActionResult> GetSexes([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetMetadata(DataTypes.Sex);
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
        /// Get all departments
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("departments")]
        [Produces(typeof(List<DepartmentModel>))]
        public async Task<IActionResult> GetDepartments([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetAllDepartments();
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
        /// Add new department
        /// </summary>
        /// <param name="department"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("departments")]
        [Produces(typeof(DepartmentModel))]
        public async Task<IActionResult> AddDepartment(DepartmentModel department, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.AddDepartment(department);
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
        /// Delete department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("departments/{departmentId}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteDepartment(departmentId);
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
        /// Get departments by procedure type
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("departments/{departmentId}/procedureTypes")]
        [Produces(typeof(List<ProcedureTypeModel>))]
        public async Task<IActionResult> GetProcedureTypesByDepartment(int departmentId, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetProcedureTypesByDepartment(departmentId);
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
        /// Get procedure types with null department
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet("procedureTypes")]
        [Produces(typeof(List<ProcedureTypeModel>))]
        public async Task<IActionResult> GetProcedureTypes([FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetProcedureTypesByDepartment(null);
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
        /// Add a procedure type
        /// </summary>
        /// <param name="procedureType"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("procedureTypes")]
        [Produces(typeof(ProcedureTypeModel))]
        public async Task<IActionResult> AddProcedureType(ProcedureTypeModel procedureType, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.AddProcedureType(procedureType);
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
        /// Delete a procedure type
        /// </summary>
        /// <param name="procedureTypeName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("procedureTypes/{procedureTypeId}")]
        public async Task<IActionResult> DeleteProcedureType(string procedureTypeName, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteProcedureType(new ProcedureTypeModel()
                {
                    Name = procedureTypeName
                });

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
        /// Delete a procedure types from a department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="procedureTypeId"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("departments/{departmentId}/procedureTypes/{procedureTypeId}")]
        public async Task<IActionResult> DeleteProcedureType(int departmentId, int procedureTypeId, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteProcedureType(new ProcedureTypeModel()
                {
                    DepartmentId = departmentId,
                    Id = procedureTypeId
                });

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