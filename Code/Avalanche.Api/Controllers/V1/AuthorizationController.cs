using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Security;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        #region private fields

        readonly ISecurityManager _securityService;
        readonly IAppLoggerService _appLoggerService;

        #endregion

        #region ctor

        public AuthorizationController(ISecurityManager securityService, IAppLoggerService appLoggerService)
        {
            _securityService = securityService;
            _appLoggerService = appLoggerService;
        }

        #endregion

        #region endpoints

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody]User user, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var token = await _securityService.Authenticate(user);

                if (string.IsNullOrWhiteSpace(token)) { return Unauthorized(); }

                return new OkObjectResult(JsonConvert.DeserializeObject<Token>(token));
            }
            catch (Exception exception)
            {
                _appLoggerService.Log(LogType.Error, LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        #endregion
    }
}