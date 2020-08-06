using Avalanche.Api.Managers.Health;
using Avalanche.Api.Managers.Settings;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.Protos;
using Ism.Telemetry.RabbitMq.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{

    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<ISettingsService> _settingsService;
        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _settingsService = new Mock<ISettingsService>();
            _manager = new PatientsManager(_pieService.Object, _settingsService.Object);
        }

        public static IEnumerable<TestCaseData> NewPatientViewModelWrongDataTestCases
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = null,
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = null,
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = null,
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = null
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = null,
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
            }
        }

        public static IEnumerable<TestCaseData> PatientUpdateViewModelWrongDataTestCases
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = null,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = null,
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = null,
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = null,
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = null
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Gender = new KeyValuePairViewModel()
                    {
                        Id = null,
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
            }
        }

        [Test, TestCaseSource(nameof(NewPatientViewModelWrongDataTestCases))]
        
        public void RegisterPatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Patient>())).ReturnsAsync(new Patient());

            Task Act() => _manager.RegisterPatient(patient);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Patient>()), Times.Never);            
        }

        [Test]

        public void RegisterPatientWorksWithRightData()
        {
            PatientViewModel patient = new PatientViewModel()
            {
                MRN = "Sample",
                DateOfBirth = DateTime.Today,
                FirstName = "Sample",
                LastName = "Sample",
                Gender = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                }
            };

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Patient>())).ReturnsAsync(new Patient());

            var result = _manager.RegisterPatient(patient);

            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Patient>()), Times.Once);
        }

        [Test]
        public void QuickPatientRegistrationWorks()
        {
            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Patient>())).ReturnsAsync(new Patient());

            var result = _manager.QuickPatientRegistration();

            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Patient>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(PatientUpdateViewModelWrongDataTestCases))]

        public void UpdatePatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<Patient>()));

            Task Act() => _manager.UpdatePatient(patient);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<Patient>()), Times.Never);
        }

        [Test]

        public void UpdatePatientWorksWithRightData()
        {
            PatientViewModel patient = new PatientViewModel()
            {
                Id = 0,
                MRN = "Sample",
                DateOfBirth = DateTime.Today,
                FirstName = "Sample",
                LastName = "Sample",
                Gender = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                }
            };

            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<Patient>()));

            var result = _manager.UpdatePatient(patient);

            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<Patient>()), Times.Once);
        }

        [Test]
        public void DeleteWorks()
        {
            _pieService.Setup(mock => mock.DeletePatient(It.IsAny<ulong>()));

            var result = _manager.DeletePatient(It.IsAny<ulong>());

            _pieService.Verify(mock => mock.DeletePatient(It.IsAny<ulong>()), Times.Once);
        }


        [Test]
        public void ExecuteSearchKeywordShouldReturnResponse()
        {
            PatientKeywordSearchFilterViewModel filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "name"
            };

            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            List<Patient> response = new List<Patient>();

            _pieService.Setup(mock => mock.Search(It.IsAny<PatientSearchFieldsMessage>(), filter.Page * filter.Limit, filter.Limit, cultureName)).ReturnsAsync(response);

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<PatientSearchFieldsMessage>(), filter.Page * filter.Limit, filter.Limit, cultureName), Times.Once);

            Assert.IsNotNull(response);
        }

        [Test]
        public void ExecuteSearchDetailsShouldReturnResponse()
        {
            PatientDetailsSearchFilterViewModel filter = new PatientDetailsSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                LastName = "Name",
                RoomName = "Room",
            };

            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            List<Patient> response = new List<Patient>();

            _pieService.Setup(mock => mock.Search(It.IsAny<PatientSearchFieldsMessage>(), filter.Page * filter.Limit, filter.Limit, cultureName)).ReturnsAsync(response);

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<PatientSearchFieldsMessage>(), filter.Page * filter.Limit, filter.Limit, cultureName), Times.Once);

            Assert.IsNotNull(response);
        }
    }
}
