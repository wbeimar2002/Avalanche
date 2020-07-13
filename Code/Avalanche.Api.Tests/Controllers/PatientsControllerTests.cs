using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
    public class PatientsControllerTests
    {
        Mock<ILogger<PatientsController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPatientsManager> _patientsManager;

        PatientsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<PatientsController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _patientsManager = new Mock<IPatientsManager>();

            _controller = new PatientsController(_appLoggerService.Object, _patientsManager.Object);

            var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
            mockUrlHelper
                .Setup(
                    x => x.Action(
                        It.IsAny<UrlActionContext>()
                    )
                )
                .Returns("https://example.com")
                .Verifiable();

            _controller.Url = mockUrlHelper.Object;

            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext = context;

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void SearchShouldReturnOkResultWithSomePatientsAsResult()
        {
            var fixture = new Fixture();
            List<Patient> list = fixture.CreateMany<Patient>(10).ToList();

            var filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "Fake"
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.Search(filter, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, "Exception PatientsController.Search", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, "Requested PatientsController.Search", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, "Completed PatientsController.Search", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchShouldReturnOkResultWithZeroListOfPatients()
        {
            List<Patient> list = new List<Patient>();

            var filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "Fake"
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.Search(filter, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Search", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Search", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Search", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientKeywordSearchFilterViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Search(It.IsAny<PatientKeywordSearchFilterViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Search", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Search", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Search", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void PostShouldReturnOkWithNewPatientInfo()
        {
            PatientViewModel patient = new PatientViewModel();
            _patientsManager.Setup(mock => mock.RegisterPatient(patient)).ReturnsAsync(new Patient());

            var okResult = _controller.Post(patient, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Post", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Post", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Post", Times.Once());
            }

            Assert.IsInstanceOf<ObjectResult>(okResult.Result);
        }

        [Test]
        public void PostShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.RegisterPatient(It.IsAny<PatientViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Post(It.IsAny<PatientViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Post", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Post", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Post", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }


    }
}
