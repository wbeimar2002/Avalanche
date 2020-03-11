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

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class NotificationsControllerTests
    {
        Mock<ILogger<NotificationsController>> _appLoggerService;
        Mock<IBroadcastService> _broadcastService;
        Mock<IWebHostEnvironment> _environment;

        NotificationsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<NotificationsController>>();
            _broadcastService = new Mock<IBroadcastService>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new NotificationsController(_broadcastService.Object, _appLoggerService.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void BroadcastShouldReturnOkResult()
        {
            var message = new MessageRequest();
            var okResult = _controller.Broadcast(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Broadcast", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Broadcast", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Broadcast", Times.Once());
            }

            Assert.IsInstanceOf<AcceptedResult>(okResult.Result);
        }

        [Test]
        public void BroadcastShouldReturnBadResultIfFails()
        {
            _broadcastService.Setup(mock => mock.Broadcast(It.IsAny<MessageRequest>())).Throws(It.IsAny<Exception>());

            var message = new MessageRequest();
            var badResult = _controller.Broadcast(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Broadcast", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Broadcast", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Broadcast", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}