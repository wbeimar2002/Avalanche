using Avalanche.Api.Managers.Health;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<ISettingsService> _settingsService;
        Mock<ILogger<PatientsManager>> _appLoggerService;

        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _settingsService = new Mock<ISettingsService>();
            _appLoggerService = new Mock<ILogger<PatientsManager>>();

            _manager = new PatientsManager(_pieService.Object);
        }

        [Test]
        public void ExecuteSearchKeywordShouldReturnResponse()
        {
            PatientKeywordSearchFilterViewModel searchModel = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "name"
            };

            List<Patient> response = new List<Patient>();

            _pieService.Setup(mock => mock.Search(It.IsAny<PatientKeywordSearchFilterViewModel>())).ReturnsAsync(response);

            var actionResult = _manager.Search(searchModel);

            _pieService.Verify(mock => mock.Search(It.IsAny<PatientKeywordSearchFilterViewModel>()), Times.Once);

            Assert.IsNotNull(response);
        }

        [Test]
        public void ExecuteSearchDetailsShouldReturnResponse()
        {
            PatientDetailsSearchFilterViewModel searchModel = new PatientDetailsSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                LastName = "Name",
                RoomName = "Room",
            };

            List<Patient> response = new List<Patient>();

            _pieService.Setup(mock => mock.Search(It.IsAny<PatientDetailsSearchFilterViewModel>())).ReturnsAsync(response);

            var actionResult = _manager.Search(searchModel);

            _pieService.Verify(mock => mock.Search(It.IsAny<PatientDetailsSearchFilterViewModel>()), Times.Once);

            Assert.IsNotNull(response);
        }
    }
}
