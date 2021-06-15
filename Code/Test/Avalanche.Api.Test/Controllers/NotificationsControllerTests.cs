using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Notifications;
using Avalanche.Api.Tests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class NotificationsControllerTests
    {
        Mock<ILogger<NotificationsController>> _logger;
        Mock<IWebHostEnvironment> _environment;
        Mock<INotificationsManager> _notificationsManager;

        NotificationsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<NotificationsController>>();
            _notificationsManager = new Mock<INotificationsManager>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new NotificationsController(_logger.Object, _notificationsManager.Object, _environment.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void SendDirectMessageShouldReturnOkResult()
        {
            var message = new Ism.Broadcaster.Models.MessageRequest();
            _notificationsManager.Setup(mock => mock.SendDirectMessage(It.IsAny<Ism.Broadcaster.Models.MessageRequest>()));

            var okResult = _controller.SendDirectMessage(message);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendDirectMessage", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendDirectMessage", Times.Once());
            }

            Assert.IsInstanceOf<AcceptedResult>(okResult);
        }

        [Test]
        public void SendDirectMessageShouldReturnBadResultIfFails()
        {
            _notificationsManager.Setup(mock => mock.SendDirectMessage(It.IsAny<Ism.Broadcaster.Models.MessageRequest>())).Throws(It.IsAny<Exception>());

            var message = new Ism.Broadcaster.Models.MessageRequest();
            var badResult = _controller.SendDirectMessage(message);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendDirectMessage", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}