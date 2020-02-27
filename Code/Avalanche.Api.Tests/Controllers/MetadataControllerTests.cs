using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Shared.Domain.Enumerations;
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
    public class MetadataControllerTests
    {
        Mock<ILogger<MetadataController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IMetadataManager> _metadataManager;

        MetadataController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<MetadataController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _metadataManager = new Mock<IMetadataManager>();

            _controller = new MetadataController(_appLoggerService.Object, _metadataManager.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void GetGendersShouldReturnOkResult()
        {
            var okResult = _controller.GetGenders(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetGenders", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetGenders", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetGenders", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetGendersShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetMetadata(MetadataTypes.Genders)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetGenders(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetGenders", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetGenders", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetGenders", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetProcedureTypesShouldReturnOkResult()
        {
            var okResult = _controller.GetProcedureTypes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypes", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetProcedureTypesShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetMetadata(MetadataTypes.ProcedureTypes)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetProcedureTypes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
