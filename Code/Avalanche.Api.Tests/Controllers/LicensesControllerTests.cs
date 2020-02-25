using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Licensing;
using Avalanche.Api.Tests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class LicensesControllerTests
    {
        Mock<ILogger<LicensesController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<ILicensingManager> _licensingManager;

        LicensesController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<LicensesController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _licensingManager = new Mock<ILicensingManager>();

            _controller = new LicensesController(_appLoggerService.Object, _licensingManager.Object) ;
        }

        [Test]
        public void ValidateShouldReturnOkResult()
        {
            string licenseKey = Guid.NewGuid().ToString();
            var okResult = _controller.Validate(licenseKey, _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception LicensesController.Validate", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested LicensesController.Validate", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed LicensesController.Validate", Times.Once());

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void ValidateShouldReturnBadResultIfFails()
        {
            _licensingManager.Setup(mock => mock.Validate(It.IsAny<string>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Validate(It.IsAny<string>(), _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception LicensesController.Validate", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Requested LicensesController.Validate", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed LicensesController.Validate", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetAllActiveShouldReturnOkResult()
        {
            string licenseKey = Guid.NewGuid().ToString();
            var okResult = _controller.GetAllActive(_environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception LicensesController.GetAllActive", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested LicensesController.GetAllActive", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed LicensesController.GetAllActive", Times.Once());

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void GetAllActiveShouldReturnBadResultIfFails()
        {
            _licensingManager.Setup(mock => mock.Validate(It.IsAny<string>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Validate(It.IsAny<string>(), _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception LicensesController.GetAllActive", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Requested LicensesController.GetAllActive", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed LicensesController.GetAllActive", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}

