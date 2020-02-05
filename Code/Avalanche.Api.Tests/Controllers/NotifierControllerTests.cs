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
using Avalanche.Api.Broadcaster.Services;
using Avalanche.Api.Broadcaster.Models;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class NotifierControllerTests
    {
        Mock<ILogger<NotifierController>> _appLoggerService;
        Mock<IBroadcastService> _broadcastService;
        Mock<IWebHostEnvironment> _environment;

        NotifierController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<NotifierController>>();
            _broadcastService = new Mock<IBroadcastService>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new NotifierController(_broadcastService.Object, _appLoggerService.Object);
        }

        [Test]
        public void BroadcastShouldReturnOkResult()
        {
            _broadcastService.Setup(mock => mock.Broadcast(It.IsAny<MessageRequest>()));

            var message = new MessageRequest();
            var okResult = _controller.Broadcast(message, _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception NotifierController.Broadcast", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested NotifierController.Broadcast", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed NotifierController.Broadcast", Times.Once());

            Assert.IsInstanceOf<AcceptedResult>(okResult.Result);
        }

        [Test]
        public void BroadcastShouldReturnBadResultIfFails()
        {
            _broadcastService.Setup(mock => mock.Broadcast(It.IsAny<MessageRequest>())).Throws(It.IsAny<Exception>());

            var message = new MessageRequest();
            var badResult = _controller.Broadcast(message, _environment.Object);

            _appLoggerService.Verify<NotifierController>(LogLevel.Error, "Exception NotifierController.Broadcast", Times.Once());
            _appLoggerService.Verify<NotifierController>(LogLevel.Debug, "Requested NotifierController.Broadcast", Times.Once());
            _appLoggerService.Verify<NotifierController>(LogLevel.Debug, "Completed NotifierController.Broadcast", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}