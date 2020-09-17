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
        Mock<ILogger<NotificationsController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<INotificationsManager> _notificationsManager;

        NotificationsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<NotificationsController>>();
            _notificationsManager = new Mock<INotificationsManager>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new NotificationsController(_appLoggerService.Object, _notificationsManager.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void SendDirectMessageShouldReturnOkResult()
        {
            var message = new Ism.Broadcaster.Models.MessageRequest();
            _notificationsManager.Setup(mock => mock.SendDirectMessage(It.IsAny<Ism.Broadcaster.Models.MessageRequest>()));

            var okResult = _controller.SendDirectMessage(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendDirectMessage", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendDirectMessage", Times.Once());
            }

            Assert.IsInstanceOf<AcceptedResult>(okResult);
        }

        [Test]
        public void SendDirectMessageShouldReturnBadResultIfFails()
        {
            _notificationsManager.Setup(mock => mock.SendDirectMessage(It.IsAny<Ism.Broadcaster.Models.MessageRequest>())).Throws(It.IsAny<Exception>());

            var message = new Ism.Broadcaster.Models.MessageRequest();
            var badResult = _controller.SendDirectMessage(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendDirectMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendDirectMessage", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public void SendQueuedMessageQueuedShouldReturnOkResult()
        {
            var message = new Ism.Broadcaster.Models.MessageRequest();
            _notificationsManager.Setup(mock => mock.SendQueuedMessage(message));

            var okResult = _controller.SendQueuedMessage(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendQueuedMessage", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendQueuedMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendQueuedMessage", Times.Once());
            }

            Assert.IsInstanceOf<AcceptedResult>(okResult);
        }

        [Test]
        public void SendQueuedMessageQueuedShouldReturnBadResultIfFails()
        {
            _notificationsManager.Setup(mock => mock.SendQueuedMessage(It.IsAny<Ism.Broadcaster.Models.MessageRequest>())).Throws(It.IsAny<Exception>()); 

            var message = new Ism.Broadcaster.Models.MessageRequest();
            var badResult = _controller.SendQueuedMessage(message, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendQueuedMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendQueuedMessage", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendQueuedMessage", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}