using System;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Security;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Avalanche.Api.ViewModels;
using System.Collections.Generic;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUsersManager _usersManager;
        private readonly IWebHostEnvironment _environment;

        public UsersController(ILogger<PhysiciansController> logger, IUsersManager usersManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _usersManager = usersManager;
        }

        /// <summary>
        /// Get users based on filter criteria
        /// </summary>
        /// <param name="filter"></param>
        [HttpPost("filtered")]
        [ProducesResponseType(typeof(List<UserViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(UserSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _usersManager.Search(filter);
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
