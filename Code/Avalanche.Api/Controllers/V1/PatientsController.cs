using System;
using System.Threading.Tasks;
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
        /// Search patient using keyword and paging
        /// </summary>
        /// <param name="filter"></param>
        [HttpPost("filtered")]
        [Produces(typeof(PagedCollectionViewModel<PatientViewModel>))]
        public async Task<IActionResult> Search([FromBody]PatientKeywordSearchFilterViewModel filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

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
