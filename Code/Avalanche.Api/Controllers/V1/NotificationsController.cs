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

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    public class NotificationsController : Controller
    {
        private readonly IBroadcastService _broadcastService;
        private readonly ILogger _appLoggerService;
        private readonly IRabbitMqClientService _rabbitMqClientService;
        private readonly RabbitMqOptions _rabbitMqOptions;

        public NotificationsController(IBroadcastService broadcastService, 
            ILogger<NotificationsController> appLoggerService,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IRabbitMqClientService rabbitMqClient)
        {
            _broadcastService = broadcastService;
            _appLoggerService = appLoggerService;
            _rabbitMqClientService = rabbitMqClient;
            _rabbitMqOptions = rabbitMqOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast([FromBody]MessageRequest messageRequest, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                Action<MessageRequest> externalAction = (messageRequest) =>
                {
                    _rabbitMqClientService.SendDirectLog(_rabbitMqOptions.QueueName, new Message()
                    {
                        Content = messageRequest.Content,
                        EventName = (Ism.RabbitMq.Client.Enumerations.EventNameEnum)messageRequest.EventName,
                    });
                };

                _broadcastService.Broadcast(messageRequest, externalAction);

                return await Task.FromResult(Accepted());
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