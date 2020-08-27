using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
        [HttpPost("")]
        [Produces(typeof(Patient))]
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

        [HttpPut("{id}")]
        [Produces(typeof(Patient))]
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

        [HttpDelete("{id}")]
        [Produces(typeof(Patient))]
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
        /// Get a quick patient registration
        /// </summary>
        [HttpPost("quick")]
        [Produces(typeof(Patient))]
        public async Task<IActionResult> QuickPatientRegistration([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var newPatient = await _patientsManager.QuickPatientRegistration(User);

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
        /// Search patient using criteria and paging
        /// </summary>
        [HttpPost("filtered")]
        [Produces(typeof(PagedCollectionViewModel<Patient>))]
        public async Task<IActionResult> Search([FromBody]PatientKeywordSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                var result = new PagedCollectionViewModel<Patient>();
                result.Items = await _patientsManager.Search(filter);

                AppendPagingContext(filter, result);
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
        /// Search patient using criteria and paging
        /// </summary>
        [HttpPost("filteredDetailed")]
        [Produces(typeof(PagedCollectionViewModel<Patient>))]
        public async Task<IActionResult> SearchDetailed([FromBody]PatientDetailsSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = new PagedCollectionViewModel<Patient>();
                result.Items = await _patientsManager.Search(filter);

                AppendPagingContext(filter, result);
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

        private void AppendPagingContext<TFilterViewModel, TResult>(TFilterViewModel filter, PagedCollectionViewModel<TResult> result)
            where TFilterViewModel : FilterViewModelBase
            where TResult : class
        {
            //TODO: Not sure the UI is consuming this at the moment.  May need to revisit paging mechanism later depending on UI implementation?

            //Get next page URL string  
            TFilterViewModel nextFilter = filter.Clone() as TFilterViewModel;
            nextFilter.Page += 1;
            String nextUrl = result.Items.Count() <= 0 ? null : this.Url.Action("Get", null, nextFilter, Request.Scheme);

            //Get previous page URL string  
            TFilterViewModel previousFilter = filter.Clone() as TFilterViewModel;
            previousFilter.Page -= 1;
            String previousUrl = previousFilter.Page <= 0 ? null : this.Url.Action("Get", null, previousFilter, Request.Scheme);

            result.NextPage = !String.IsNullOrWhiteSpace(nextUrl) ? new Uri(nextUrl) : null;
            result.PreviousPage = !String.IsNullOrWhiteSpace(previousUrl) ? new Uri(previousUrl) : null;
        }
    }
}