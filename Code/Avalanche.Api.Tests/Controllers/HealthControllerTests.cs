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

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class HealthControllerTests
    {
        Mock<ILogger<HealthController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;

        HealthController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<HealthController>>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new HealthController(_appLoggerService.Object);
        }

        [Test]
        public void HealthCheckShouldReturnOkResult()
        {
            var okResult = _controller.HealthCheck(_environment.Object);
            
            _appLoggerService.Verify(LogLevel.Error, "Exception HealthController.HealthCheck", Times.Never());
            _appLoggerService.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Requested HealthController.HealthCheck", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed HealthController.HealthCheck", Times.Once());

            Assert.IsInstanceOf<OkObjectResult>(okResult);
        }

        [Test]
        public void HealthCheckShouldReturnBadResultIfFails()
        {
            _appLoggerService.Setup(LogLevel.Debug, "Requested HealthController.HealthCheck").Throws(It.IsAny<Exception>());
            
            var badResult = _controller.HealthCheck(_environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception HealthController.HealthCheck", Times.Once());
            _appLoggerService.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested HealthController.HealthCheck", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed HealthController.HealthCheck", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}