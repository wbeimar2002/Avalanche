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
        private readonly ILogger _logger;
        private readonly INotificationsManager _notificationsManager;
        private readonly IWebHostEnvironment _environment;

        public NotificationsController(
            ILogger<NotificationsController> logger,
            INotificationsManager notificationsManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _notificationsManager = notificationsManager;
            _logger = logger;
        }

        /// <summary>
        /// Send broadcast to Signal R
        /// </summary>
        /// <param name="messageRequest"></param>
        /// <returns></returns>
        [HttpPost("direct")]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        public IActionResult SendDirectMessage([FromBody]Ism.Broadcaster.Models.MessageRequest messageRequest)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _notificationsManager.SendDirectMessage(messageRequest);

                return Accepted();
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
