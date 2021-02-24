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
        readonly ILogger _appLoggerService;
        readonly IPatientsManager _patientsManager;

        public PatientsController(ILogger<PatientsController> appLoggerService, IPatientsManager patientsManager)
        {
            _appLoggerService = appLoggerService;
            _patientsManager = patientsManager;
        }

        /// <summary>
        /// Register new patient
        /// </summary>
        /// <param name="newPatient"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> ManualPatientRegistration(PatientViewModel newPatient, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var patientRegistered = await _patientsManager.RegisterPatient(newPatient);
                
                return new ObjectResult(patientRegistered) { StatusCode = StatusCodes.Status201Created };
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
        /// Update patient
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> UpdatePatient(PatientViewModel existing, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _patientsManager.UpdatePatient(existing);

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
        /// Delete patient
        /// </summary>
        /// <param name="id"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> DeletePatient(ulong id, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _patientsManager.DeletePatient(id);

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
        /// Quick patient registration
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("quick")]
        [Produces(typeof(PatientViewModel))]
        public async Task<IActionResult> QuickPatientRegistration([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var newPatient = await _patientsManager.QuickPatientRegistration();

                return new ObjectResult(newPatient) { StatusCode = StatusCodes.Status201Created };
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
        /// Search patient using keyword and paging
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("filtered")]
        [Produces(typeof(PagedCollectionViewModel<PatientViewModel>))]
        public async Task<IActionResult> Search([FromBody]PatientKeywordSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                var result = new PagedCollectionViewModel<PatientViewModel>();
                result.Items = await _patientsManager.Search(filter);

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, result);
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
        /// Search patient using criterias and paging
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("filteredDetailed")]
        [Produces(typeof(PagedCollectionViewModel<PatientViewModel>))]
        public async Task<IActionResult> SearchDetailed([FromBody]PatientDetailsSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = new PagedCollectionViewModel<PatientViewModel>();
                result.Items = await _patientsManager.Search(filter);

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, result);
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