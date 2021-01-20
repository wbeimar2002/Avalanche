using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
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
        Mock<IMediaManager> _mediaManager;

        MetadataController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<MetadataController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _metadataManager = new Mock<IMetadataManager>();
            _mediaManager = new Mock<IMediaManager>();

            _controller = new MetadataController(_appLoggerService.Object, _metadataManager.Object, _mediaManager.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void GetSexesShouldReturnOkResult()
        {
            var okResult = _controller.GetSexes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetSexes", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetSexes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetSexes", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetSexesShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetMetadata(It.IsAny<User>(), MetadataTypes.Sex)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetSexes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetSexes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetSexes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetSexes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetContentTypesShouldReturnOkResult()
        {
            var okResult = _controller.GetPgsVideoFiles(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetContentTypes", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetContentTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetContentTypes", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetContentTypesShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetMetadata(It.IsAny<User>(), MetadataTypes.PgsVideoFiles)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetPgsVideoFiles(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetContentTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetContentTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetContentTypes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetSourceTypesShouldReturnOkResult()
        {
            var okResult = _controller.GetSourceTypes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetSourceTypes", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetSourceTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetSourceTypes", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetSourceTypesShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetMetadata(It.IsAny<User>(), MetadataTypes.SourceTypes)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetSourceTypes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetSourceTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetSourceTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetSourceTypes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetDepartmentsShouldReturnOkResult()
        {
            var okResult = _controller.GetDepartments(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetDepartments", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetDepartments", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetDepartments", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetDepartmentsShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetAllDepartments(It.IsAny<User>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetDepartments(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetDepartments", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetDepartments", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetDepartments", Times.Once());
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
            _metadataManager.Setup(mock => mock.GetProceduresByDepartment(It.IsAny<User>(), null)).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetProcedureTypes(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypes", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetProceduresByDepartmentShouldReturnOkResult()
        {
            var okResult = _controller.GetProcedureTypesByDepartment(It.IsAny<int>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetProcedureTypesByDepartmentShouldReturnBadResultIfFails()
        {
            _metadataManager.Setup(mock => mock.GetProceduresByDepartment(It.IsAny<User>(), It.IsAny<int>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetProcedureTypesByDepartment(It.IsAny<int>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetProcedureTypesByDepartment", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void AddDepartmentShouldReturnOkResult()
        {
            var okResult = _controller.AddDepartment(It.IsAny<Department>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.AddDepartment", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.AddDepartment", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.AddDepartment", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void AddProcedureTypeShouldReturnOkResult()
        {
            var okResult = _controller.AddProcedureType(It.IsAny<ProcedureType>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.AddProcedureType", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.AddProcedureType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.AddProcedureType", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void DeleteDepartmentShouldReturnOkResult()
        {
            var okResult = _controller.DeleteDepartment(It.IsAny<int>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeleteDepartment", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeleteDepartment", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeleteDepartment", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void DeleteProcedureTypeShouldReturnOkResult()
        {
            var okResult = _controller.DeleteProcedureType(It.IsAny<string>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeleteProcedureType", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeleteProcedureType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeleteProcedureType", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void DeleteProcedureTypeByDepartmentShouldReturnOkResult()
        {
            var okResult = _controller.DeleteProcedureType(It.IsAny<int>(), It.IsAny<int>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeleteProcedureType", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeleteProcedureType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeleteProcedureType", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }
    }
}
