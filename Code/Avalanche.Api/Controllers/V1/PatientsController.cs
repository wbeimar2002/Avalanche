using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IPatientsManager _patientsManager;

        public PatientsController(ILogger<PatientsController> appLoggerService, IPatientsManager patientsManager)
        {
            _appLoggerService = appLoggerService;
            _patientsManager = patientsManager;
        }

        [HttpPost("")]
        [Produces(typeof(Patient))]
        public async Task<IActionResult> Post(Patient newPatient, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                newPatient = await _patientsManager.RegisterPatient(newPatient);
                return Ok(newPatient);
            }
            catch (Exception exception)
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpPost("quick")]
        [Produces(typeof(Patient))]
        public async Task<IActionResult> Post([FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var newPatient = await _patientsManager.RegisterQuickPatient();
                return Ok(newPatient);
            }
            catch (Exception exception)
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpPost("filtered")]
        [Produces(typeof(List<Patient>))]
        public async Task<IActionResult> Search(PatientSearchFilterViewModel filter, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _patientsManager.Search(filter);
                return Ok(result);
            }
            catch (Exception exception)
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

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
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

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
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}