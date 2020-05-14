using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task<IActionResult> Post(Patient newPatient, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                newPatient = await _patientsManager.RegisterPatient(newPatient);
                
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
        /// Get a quick patient registration
        /// </summary>
        [HttpPost("quick")]
        [Produces(typeof(Patient))]
        public async Task<IActionResult> Post([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var newPatient = await _patientsManager.RegisterQuickPatient();

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
        public async Task<IActionResult> Search([FromBody]PatientSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                
                var result = new PagedCollectionViewModel<Patient>();
                result.Items = await _patientsManager.Search(filter);

                //Get next page URL string  
                PatientSearchFilterViewModel nextFilter = filter.Clone() as PatientSearchFilterViewModel;
                nextFilter.Page += 1;
                String nextUrl = result.Items.Count() <= 0 ? null : this.Url.Action("Get", null, nextFilter, Request.Scheme);

                //Get previous page URL string  
                PatientSearchFilterViewModel previousFilter = filter.Clone() as PatientSearchFilterViewModel;
                previousFilter.Page -= 1;
                String previousUrl = previousFilter.Page <= 0 ? null : this.Url.Action("Get", null, previousFilter, Request.Scheme);

                result.NextPage = !String.IsNullOrWhiteSpace(nextUrl) ? new Uri(nextUrl) : null;
                result.PreviousPage = !String.IsNullOrWhiteSpace(previousUrl) ? new Uri(previousUrl) : null;

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
        /// Get physiciansby patient
        /// </summary>
        [HttpGet("{patiendId}/physicians/")]
        [Produces(typeof(List<Physician>))]
        public async Task<IActionResult> GetPhysiciansByPatient(string patiendId, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _patientsManager.GetPhysiciansByPatient(patiendId);
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
        /// Get procedures by patient and physician
        /// </summary>
        [HttpGet("{patientId}/physicians/{physicianId}/procedures")]
        [Produces(typeof(List<Procedure>))]
        public async Task<IActionResult> GetProceduresByPhysicianAndPatient(string patiendId, string physicianId, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _patientsManager.GetProceduresByPhysicianAndPatient(patiendId, physicianId);
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