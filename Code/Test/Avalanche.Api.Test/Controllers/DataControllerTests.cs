using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class DataControllerTests
    {
        Mock<ILogger<DataController>> _logger;
        Mock<IWebHostEnvironment> _environment;
        Mock<IDataManager> _metadataManager;

        DataController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<DataController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _metadataManager = new Mock<IDataManager>();

            _controller = new DataController(_logger.Object, _metadataManager.Object, _environment.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void GetDepartmentsShouldReturnOkResult()
        {
            var okResult = _controller.GetDepartments();

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetDepartments", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetDepartments", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetDepartments", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetDepartmentsShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetAllDepartments()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetDepartments();

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetDepartments", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetDepartments", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetDepartments", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetProcedureTypesShouldReturnOkResult()
        {
            var okResult = _controller.GetProcedureTypes();

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypes", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetProcedureTypesShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetProcedureTypesByDepartment(null)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetProcedureTypes();

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetProceduresByDepartmentShouldReturnOkResult()
        {
            var okResult = _controller.GetProcedureTypesByDepartment(It.IsAny<int>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetProcedureTypesByDepartmentShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetProcedureTypesByDepartment(It.IsAny<int>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetProcedureTypesByDepartment(It.IsAny<int>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetLabelsByProcedureTypeShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetLabelsByProcedureType(It.IsAny<int>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetLabelsByProcedureType(It.IsAny<int>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetLabelsByProcedureTypeShouldReturnOkResult()
        {
            var okResult = _controller.GetLabelsByProcedureType(It.IsAny<int>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetLabelsByProcedureType", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }
    }
}
