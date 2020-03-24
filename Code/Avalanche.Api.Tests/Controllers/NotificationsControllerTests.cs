using Avalanche.Api.Controllers.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using Avalanche.Api.Tests.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ism.Broadcaster.Services;
using Ism.Broadcaster.Models;
using Ism.RabbitMq.Client;
using Ism.RabbitMq.Client.Models;
using Microsoft.Extensions.Options;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class NotificationsControllerTests
    {
        Mock<ILogger<NotificationsController>> _appLoggerService;
        Mock<IBroadcastService> _broadcastService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IRabbitMqClientService> _rabbitMqClientService;
        Mock<IOptions<RabbitMqOptions>> _rabbitMqOptions;

        NotificationsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<NotificationsController>>();
            _broadcastService = new Mock<IBroadcastService>();
            _rabbitMqClientService = new Mock<IRabbitMqClientService>();
            _rabbitMqOptions = new Mock<IOptions<RabbitMqOptions>>();

            _environment = new Mock<IWebHostEnvironment>();

            _controller = new NotificationsController(_broadcastService.Object, _appLoggerService.Object, _rabbitMqOptions.Object, _rabbitMqClientService.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void BroadcastShouldReturnOkResult()
        {
            var message = new Ism.Broadcaster.Models.MessageRequest();

            _broadcastService.Setup(mock => mock.Broadcast(It.IsAny<Ism.Broadcaster.Models.MessageRequest>()));

            var okResult = _controller.BroadcastDirect(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.BroadcastDirect", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.BroadcastDirect", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.BroadcastDirect", Times.Once());
            }

            Assert.IsInstanceOf<AcceptedResult>(okResult);
        }

        [Test]
        public void BroadcastShouldReturnBadResultIfFails()
        {
            _broadcastService.Setup(mock => mock.Broadcast(It.IsAny<Ism.Broadcaster.Models.MessageRequest>())).Throws(It.IsAny<Exception>());

            var message = new Ism.Broadcaster.Models.MessageRequest();
            var badResult = _controller.BroadcastDirect(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.BroadcastDirect", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.BroadcastDirect", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.BroadcastDirect", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}