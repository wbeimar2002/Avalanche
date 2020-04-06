using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ism.Broadcaster.Models;
using Ism.Broadcaster.Services;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ism.RabbitMq.Client;
using Microsoft.Extensions.Options;
using Ism.RabbitMq.Client.Models;
using Microsoft.AspNetCore.Cors;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
        public class NotificationsController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IRabbitMqClientService _rabbitMqClientService;
        readonly IBroadcastService _broadcastService;
        readonly RabbitMqOptions _rabbitMqOptions;

        public NotificationsController(IBroadcastService broadcastService,
            ILogger<NotificationsController> appLoggerService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IRabbitMqClientService rabbitMqClient)
        {
            _appLoggerService = appLoggerService;
            _rabbitMqClientService = rabbitMqClient;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _broadcastService = broadcastService;
        }

        [HttpPost("queue")]
        public IActionResult Broadcast([FromBody]Ism.Broadcaster.Models.MessageRequest messageRequest, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                _rabbitMqClientService.SendMessage(_rabbitMqOptions.QueueName, messageRequest.Json());

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

        [HttpPost("direct")]
        public IActionResult BroadcastDirect([FromBody]Ism.Broadcaster.Models.MessageRequest messageRequest, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _broadcastService.Broadcast(messageRequest);

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