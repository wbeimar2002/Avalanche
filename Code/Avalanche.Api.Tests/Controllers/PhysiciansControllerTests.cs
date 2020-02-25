using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Shared.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class PhysiciansControllerTests
    {
        Mock<ILogger<PhysiciansController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPhysiciansManager> _physiciansManager;

        PhysiciansController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<PhysiciansController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _physiciansManager = new Mock<IPhysiciansManager>();

            _controller = new PhysiciansController(_appLoggerService.Object, _physiciansManager.Object);
        }

        [Test]
        public void BroadcastShouldReturnOkResult()
        {
            var fixture = new Fixture();
            var list = fixture.CreateMany<Physician>(10).ToList();

            _physiciansManager.Setup(mock => mock.GetAllPhysicians()).ReturnsAsync(list);

            var okResult = _controller.GetAll(_environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception PhysiciansController.GetAll", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested PhysiciansController.GetAll", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed PhysiciansController.GetAll", Times.Once());

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void BroadcastShouldReturnBadResultIfFails()
        {
            _physiciansManager.Setup(mock => mock.GetAllPhysicians()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetAll(_environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception PhysiciansController.GetAll", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Requested PhysiciansController.GetAll", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed PhysiciansController.GetAll", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
