using System;
using System.Threading.Tasks;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Procedures;
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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProceduresController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IProceduresManager _proceduresManager;
        private readonly IWebHostEnvironment _environment;

        public ProceduresController(ILogger<ProceduresController> logger, IProceduresManager proceduresManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _proceduresManager = proceduresManager;
        }

        /// <summary>
        /// Search procedures with advanced and basic filters
        /// </summary>
        /// <param name="filter"></param>
        [HttpPost("filtered")]
        [ProducesResponseType(typeof(ProceduresContainerReponseViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(ProcedureSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _proceduresManager.Search(filter);

                var procedures = new PagedCollectionViewModel<ProcedureViewModel>
                {
                    Items = result.Procedures
                };

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, procedures);

                return Ok(new ProceduresContainerReponseViewModel { TotalCount = result.TotalCount, PagedProcedures = procedures });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Search procedures by patient
        /// </summary>
        /// <param name="patientId"></param>
        [HttpGet("patients/{patientId}")]
        [ProducesResponseType(typeof(ProceduresContainerViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByPatient(string patientId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _proceduresManager.SearchByPatient(patientId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Get procedure
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProcedureViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _proceduresManager.GetProcedureDetails(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ProcedureViewModel procedureViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _proceduresManager.UpdateProcedure(procedureViewModel);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        /// <returns>Active Procedure model or null</returns>
        [HttpGet("current")]
        [ProducesResponseType(typeof(ActiveProcedureViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _proceduresManager.GetActiveProcedure();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}
