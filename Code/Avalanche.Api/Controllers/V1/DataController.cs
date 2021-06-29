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

        [HttpGet("{sourceKey}")]
        public async Task<IActionResult> GetList(string sourceKey)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetList(sourceKey);
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

                var result = await _metadataManager.GetList(sourceKey, jsonKey);
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

        /// <summary>
        /// Add a label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        [HttpPost("labels")]
        [Produces(typeof(LabelModel))]
        public async Task<IActionResult> AddLabel(LabelModel label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.AddLabel(label);
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
        /// Delete a label
        /// </summary>
        /// <param name="labelName"></param>
        /// <returns></returns>
        [HttpDelete("labels/{labelId}")]
        public async Task<IActionResult> DeleteLabel(string labelName)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteLabel(new LabelModel()
                {
                    Name = labelName
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
        /// Delete a labels from a procedure type
        /// </summary>
        /// <param name="procedureTypeId"></param>
        /// <param name="labelId"></param>
        /// <returns></returns>
        [HttpDelete("procedureTypes/{procedureTypeId}/labels/{labelId}")]
        public async Task<IActionResult> DeleteLabel(int procedureTypeId, int labelId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _metadataManager.DeleteLabel(new LabelModel()
                {
                    ProcedureTypeId = procedureTypeId,
                    Id = labelId
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
        /// Get labels by procedure type
        /// </summary>
        /// <param name="procedureTypeId"></param>
        /// <returns></returns>
        [HttpGet("procedureTypes/{procedureTypeId}/labels")]
        [Produces(typeof(List<LabelModel>))]
        public async Task<IActionResult> GetLabelsByProcedureType(int procedureTypeId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetLabelsByProcedureType(procedureTypeId);
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
        /// Get labels with null/empty procedure type
        /// </summary>
        /// <returns></returns>
        [HttpGet("labels")]
        [Produces(typeof(List<LabelModel>))]
        public async Task<IActionResult> GetLabels()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _metadataManager.GetLabelsByProcedureType(null);
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