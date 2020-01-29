using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Broadcaster.Models;
using Avalanche.Api.Broadcaster.Services;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotifierController : Controller
    {
        private readonly IBroadcastService _broadcastService;
        private readonly ILogger _appLoggerService;

        public NotifierController(IBroadcastService broadcastService, ILogger<NotifierController> appLoggerService)
        {
            _broadcastService = broadcastService;
            _appLoggerService = appLoggerService;
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast([FromBody]MessageRequest messageRequest)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _broadcastService.Broadcast(messageRequest);

                return await Task.FromResult(Accepted());
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError (LoggerHelper.GetLogMessage(DebugLogType.Completed), ex);
                return await Task.FromResult(BadRequest());
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}