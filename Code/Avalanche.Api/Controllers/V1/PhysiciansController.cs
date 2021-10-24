using System;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Data;
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
    public class PhysiciansController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPhysiciansManager _physiciansManager;
        private readonly IWebHostEnvironment _environment;

        public PhysiciansController(ILogger<PhysiciansController> logger, IPhysiciansManager physiciansManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _physiciansManager = physiciansManager;
        }

        /// <summary>
        /// Get all physicians
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> GetAllPhysicians()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _physiciansManager.GetPhysicians();
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
