﻿using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Licensing;
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
    public class LicensesControllerTests
    {
        Mock<ILogger<LicensesController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<ILicensingManager> _licensingManager;

        LicensesController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<LicensesController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _licensingManager = new Mock<ILicensingManager>();

            _controller = new LicensesController(_appLoggerService.Object, _licensingManager.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void ValidateShouldReturnOkResult()
        {
            string licenseKey = Guid.NewGuid().ToString();
            var okResult = _controller.Validate(licenseKey, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Validate", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Validate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Validate", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void ValidateShouldReturnBadResultIfFails()
        {
            _licensingManager.Setup(mock => mock.Validate(It.IsAny<string>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Validate(It.IsAny<string>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Validate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Validate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Validate", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetAllActiveShouldReturnOkResult()
        {
            string licenseKey = Guid.NewGuid().ToString();

            var fixture = new Fixture();
            var list = fixture.CreateMany<License>(10).ToList();

            _licensingManager.Setup(mock => mock.GetAllActive()).ReturnsAsync(list);

            var okResult = _controller.GetAllActive(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetAllActive", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetAllActive", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetAllActive", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetAllActiveShouldReturnBadResultIfFails()
        {
            _licensingManager.Setup(mock => mock.GetAllActive()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetAllActive(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetAllActive", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetAllActive", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetAllActive", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
