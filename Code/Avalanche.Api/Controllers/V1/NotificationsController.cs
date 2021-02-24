using Avalanche.Api.Managers.Notifications;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class NotificationsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly INotificationsManager _notificationsManager;

        public NotificationsController(
            ILogger<NotificationsController> appLoggerService,
            INotificationsManager notificationsManager)
        {
            _notificationsManager = notificationsManager;
            _appLoggerService = appLoggerService;
        }

        /// <summary>
        /// Send broadcast to Signal R
        /// </summary>
        /// <param name="messageRequest"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpPost("direct")]
        public IActionResult SendDirectMessage([FromBody]Ism.Broadcaster.Models.MessageRequest messageRequest, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _notificationsManager.SendDirectMessage(messageRequest);

                return Accepted();
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