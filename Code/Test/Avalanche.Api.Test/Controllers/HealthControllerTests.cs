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
        Mock<ILogger<HealthController>> _logger;
        Mock<IWebHostEnvironment> _environment;

        HealthController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<HealthController>>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new HealthController(_logger.Object, _environment.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void HealthCheckShouldReturnOkResult()
        {
            var okResult = _controller.HealthCheck(_environment.Object);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.HealthCheck", Times.Never());
                _logger.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.HealthCheck", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.HealthCheck", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult);
        }

        [Test]
        public void HealthCheckShouldReturnBadResultIfFails()
        {
            if (_checkLogger)
            {
                _logger.Setup(LogLevel.Debug, "Requested HealthController.HealthCheck").Throws(It.IsAny<Exception>());
            
                var badResult = _controller.HealthCheck(_environment.Object);

                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.HealthCheck", Times.Once());
                _logger.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.HealthCheck", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.HealthCheck", Times.Once());
            
                Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
            }
        }

        [Test]
        public void HealthCheckSecureShouldReturnOkResult()
        {
            var okResult = _controller.HealthCheckSecure(_environment.Object);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.HealthCheckSecure", Times.Never());
                _logger.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.HealthCheckSecure", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.HealthCheckSecure", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult);
        }

        [Test]
        public void HealthCheckSecureShouldReturnBadResultIfFails()
        {
            if (_checkLogger)
            {
                _logger.Setup(LogLevel.Debug, "Requested HealthController.HealthCheckSecure").Throws(It.IsAny<Exception>());

                var badResult = _controller.HealthCheckSecure(_environment.Object);


                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.HealthCheckSecure", Times.Once());
                _logger.Verify(LogLevel.Information, "Avalanche Api is healthy.", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.HealthCheckSecure", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.HealthCheckSecure", Times.Once());

                Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
            }
        }
    }
}