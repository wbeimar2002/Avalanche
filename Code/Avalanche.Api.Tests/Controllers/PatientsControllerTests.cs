using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
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
    public class PatientsControllerTests
    {
        Mock<ILogger<PatientsController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPatientsManager> _patientsManager;

        PatientsController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<PatientsController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _patientsManager = new Mock<IPatientsManager>();

            _controller = new PatientsController(_appLoggerService.Object, _patientsManager.Object);
        }

        [Test]
        public void SearchShouldReturnOkResult()
        {
            List<Patient> list = new List<Patient>();
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientSearchFilterViewModel>())).ReturnsAsync(list);

            var filter = new PatientSearchFilterViewModel();
            var okResult = _controller.Search(filter, _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception PatientsController.Search", Times.Never());
            _appLoggerService.Verify(LogLevel.Debug, "Requested PatientsController.Search", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed PatientsController.Search", Times.Once());

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientSearchFilterViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Search(It.IsAny<PatientSearchFilterViewModel>(), _environment.Object);

            _appLoggerService.Verify(LogLevel.Error, "Exception PatientsController.Search", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Requested PatientsController.Search", Times.Once());
            _appLoggerService.Verify(LogLevel.Debug, "Completed PatientsController.Search", Times.Once());

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
