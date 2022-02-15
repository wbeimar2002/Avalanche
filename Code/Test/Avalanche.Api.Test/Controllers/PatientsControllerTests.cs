using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
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
using System.Security.Claims;
using System.Text;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class PatientsControllerTests
    {
        Mock<ILogger<PatientsController>> _logger;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPatientsManager> _patientsManager;

        PatientsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<PatientsController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _patientsManager = new Mock<IPatientsManager>();

            _controller = new PatientsController(_logger.Object, _patientsManager.Object, _environment.Object);

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
        public void SearchDetailedShouldReturnOkResultWithSomePatientsAsResult()
        {
            var fixture = new Fixture();
            List<PatientViewModel> list = fixture.CreateMany<PatientViewModel>(10).ToList();

            var filter = new PatientDetailsSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.SearchDetailed(filter);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, "Exception PatientsController.SearchDetailed", Times.Never());
                _logger.Verify(LogLevel.Debug, "Requested PatientsController.SearchDetailed", Times.Once());
                _logger.Verify(LogLevel.Debug, "Completed PatientsController.SearchDetailed", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchDetailedShouldReturnOkResultWithZeroListOfPatients()
        {
            List<PatientViewModel> list = new List<PatientViewModel>();

            var filter = new PatientDetailsSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.SearchDetailed(filter);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SearchDetailed", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SearchDetailed", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchDetailedShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientDetailsSearchFilterViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.SearchDetailed(It.IsAny<PatientDetailsSearchFilterViewModel>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SearchDetailed", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void SearchShouldReturnOkResultWithSomePatientsAsResult()
        {
            var fixture = new Fixture();
            List<PatientViewModel> list = fixture.CreateMany<PatientViewModel>(10).ToList();

            var filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "Fake"
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.Search(filter);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, "Exception PatientsController.Search", Times.Never());
                _logger.Verify(LogLevel.Debug, "Requested PatientsController.Search", Times.Once());
                _logger.Verify(LogLevel.Debug, "Completed PatientsController.Search", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchShouldReturnOkResultWithZeroListOfPatients()
        {
            List<PatientViewModel> list = new List<PatientViewModel>();

            var filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "Fake"
            };

            _patientsManager.Setup(mock => mock.Search(filter)).ReturnsAsync(list);

            var okResult = _controller.Search(filter);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Search", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Search", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Search", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientKeywordSearchFilterViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Search(It.IsAny<PatientKeywordSearchFilterViewModel>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Search", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Search", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Search", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void UpdatePatientShouldReturnOk()
        {
            PatientViewModel patientUpdated = new PatientViewModel();
            _patientsManager.Setup(mock => mock.UpdatePatient(patientUpdated));

            var okResult = _controller.UpdatePatient(patientUpdated);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.UpdatePatient", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.UpdatePatient", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void UpdatePatientShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.UpdatePatient(It.IsAny<PatientViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.UpdatePatient(It.IsAny<PatientViewModel>());

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.UpdatePatient", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void DeletePatientShouldReturnOk()
        {
            ulong patientId = default(ulong);
            _patientsManager.Setup(mock => mock.DeletePatient(patientId));

            var okResult = _controller.DeletePatient(patientId);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeletePatient", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeletePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeletePatient", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void DeletePatientShouldReturnBadResultIfFails()
        {
            ulong patientId = default(ulong);
            _patientsManager.Setup(mock => mock.DeletePatient(patientId)).Throws(It.IsAny<Exception>());

            var badResult = _controller.DeletePatient(patientId);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeletePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeletePatient", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeletePatient", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

    }
}
