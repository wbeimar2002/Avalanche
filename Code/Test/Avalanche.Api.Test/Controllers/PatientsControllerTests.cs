﻿using AutoFixture;
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
using System.Security.Claims;
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

            var okResult = _controller.SearchDetailed(filter, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, "Exception PatientsController.SearchDetailed", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, "Requested PatientsController.SearchDetailed", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, "Completed PatientsController.SearchDetailed", Times.Once());
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

            var okResult = _controller.SearchDetailed(filter, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SearchDetailed", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SearchDetailed", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SearchDetailedShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.Search(It.IsAny<PatientDetailsSearchFilterViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.SearchDetailed(It.IsAny<PatientDetailsSearchFilterViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SearchDetailed", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SearchDetailed", Times.Once());
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
            List<PatientViewModel> list = new List<PatientViewModel>();

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
        public void RegisterPatientShouldReturnOkWithNewPatientInfo()
        {
            PatientViewModel patient = new PatientViewModel();
            _patientsManager.Setup(mock => mock.RegisterPatient(patient, It.IsAny<User>())).ReturnsAsync(new PatientViewModel());

            var okResult = _controller.ManualPatientRegistration(patient, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ManualPatientRegistration", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ManualPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ManualPatientRegistration", Times.Once());
            }

            Assert.IsInstanceOf<ObjectResult>(okResult.Result);
        }

        [Test]
        public void RegisterPatientShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.RegisterPatient(It.IsAny<PatientViewModel>(), It.IsAny<User>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.ManualPatientRegistration(It.IsAny<PatientViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ManualPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ManualPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ManualPatientRegistration", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void QuickRegistrationShouldReturnOkWithNewPatientInfo()
        {
            _patientsManager.Setup(mock => mock.QuickPatientRegistration(It.IsAny<User>())).ReturnsAsync(new PatientViewModel());

            var okResult = _controller.QuickPatientRegistration(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.QuickPatientRegistration", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.QuickPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.QuickPatientRegistration", Times.Once());
            }

            Assert.IsInstanceOf<ObjectResult>(okResult.Result);
        }

        [Test]
        public void QuickRegistrationShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.QuickPatientRegistration(It.IsAny<User>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.QuickPatientRegistration(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.QuickPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.QuickPatientRegistration", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.QuickPatientRegistration", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void UpdatePatientShouldReturnOk()
        {
            PatientViewModel patientUpdated = new PatientViewModel();
            _patientsManager.Setup(mock => mock.UpdatePatient(patientUpdated, It.IsAny<User>()));

            var okResult = _controller.UpdatePatient(patientUpdated, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.UpdatePatient", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.UpdatePatient", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void UpdatePatientShouldReturnBadResultIfFails()
        {
            _patientsManager.Setup(mock => mock.UpdatePatient(It.IsAny<PatientViewModel>(), It.IsAny<User>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.UpdatePatient(It.IsAny<PatientViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.UpdatePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.UpdatePatient", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void DeletePatientShouldReturnOk()
        {
            ulong patientId = default(ulong);
            _patientsManager.Setup(mock => mock.DeletePatient(patientId));

            var okResult = _controller.DeletePatient(patientId, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeletePatient", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeletePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeletePatient", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult.Result);
        }

        [Test]
        public void DeletePatientShouldReturnBadResultIfFails()
        {
            ulong patientId = default(ulong);
            _patientsManager.Setup(mock => mock.DeletePatient(patientId)).Throws(It.IsAny<Exception>());

            var badResult = _controller.DeletePatient(patientId, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DeletePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DeletePatient", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DeletePatient", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

    }
}