using Avalanche.Api.Extensions;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Patients;
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
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPatientsManager _patientsManager;
        private readonly IWebHostEnvironment _environment;

        public PatientsController(ILogger<PatientsController> logger, IPatientsManager patientsManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _patientsManager = patientsManager;
        }

        /// <summary>
        /// Register new patient
        /// </summary>
        /// <param name="newPatient"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PatientViewModel))]
        public async Task<IActionResult> ManualPatientRegistration(PatientViewModel newPatient)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var patientRegistered = await _patientsManager.RegisterPatient(newPatient);
                
                return new ObjectResult(patientRegistered) { StatusCode = StatusCodes.Status201Created };
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
        /// Update patient
        /// </summary>
        /// <param name="existing"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> UpdatePatient(PatientViewModel existing)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _patientsManager.UpdatePatient(existing);

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
        /// Delete patient
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> DeletePatient(ulong id)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _patientsManager.DeletePatient(id);

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
        /// Quick patient registration
        /// </summary>
        /// <returns></returns>
        [HttpPost("quick")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PatientViewModel))]
        public async Task<IActionResult> QuickPatientRegistration()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var newPatient = await _patientsManager.QuickPatientRegistration();

                return new ObjectResult(newPatient) { StatusCode = StatusCodes.Status201Created };
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
        /// Search patient using keyword and paging
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("filtered")]
        [Produces(typeof(PagedCollectionViewModel<PatientViewModel>))]
        public async Task<IActionResult> Search([FromBody]PatientKeywordSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = new PagedCollectionViewModel<PatientViewModel>
                {
                    Items = await _patientsManager.Search(filter)
                };

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, result);
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
        /// Search patient using criterias and paging
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("filteredDetailed")]
        [Produces(typeof(PagedCollectionViewModel<PatientViewModel>))]
        public async Task<IActionResult> SearchDetailed([FromBody]PatientDetailsSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = new PagedCollectionViewModel<PatientViewModel>
                {
                    Items = await _patientsManager.Search(filter)
                };

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, result);
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