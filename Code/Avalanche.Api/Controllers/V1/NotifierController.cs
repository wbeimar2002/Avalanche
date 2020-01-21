using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Broadcaster.Models;
using Avalanche.Api.Broadcaster.Services;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Services.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotifierController : Controller
    {
        private readonly IBroadcastService _broadcastService;
        private readonly IAppLoggerService _appLoggerService;

        public NotifierController(IBroadcastService broadcastService, IAppLoggerService appLoggerService)
        {
            _broadcastService = broadcastService;
            _appLoggerService = appLoggerService;
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast([FromBody]MessageRequest messageRequest)
        {
            try
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _broadcastService.Broadcast(messageRequest);

                return await Task.FromResult(Accepted());
            }
            catch (Exception ex)
            {
                _appLoggerService.Log(LogType.Error, LoggerHelper.GetLogMessage(DebugLogType.Completed), ex);
                return await Task.FromResult(BadRequest());
            }
            finally
            {
                _appLoggerService.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}