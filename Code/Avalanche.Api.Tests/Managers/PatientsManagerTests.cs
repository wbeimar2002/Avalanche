using AutoMapper;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{

    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<ISettingsService> _settingsService;
        Mock<IAccessInfoFactory> _accessInfoFactory;

        IMapper _mapper;
        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _settingsService = new Mock<ISettingsService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PieMappingConfigurations());
            });

            _mapper = config.CreateMapper();
            _manager = new PatientsManager(_pieService.Object, _settingsService.Object, _accessInfoFactory.Object, _mapper);
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = null
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = new KeyValuePairViewModel()
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
                    Sex = null
                });
                yield return new TestCaseData(new PatientViewModel()
                {
                    Id = 0,
                    MRN = "Sample",
                    DateOfBirth = DateTime.Today,
                    FirstName = "Sample",
                    LastName = "Sample",
                    Sex = new KeyValuePairViewModel()
                    {
                        Id = null,
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
            }
        }

        [Test, TestCaseSource(nameof(NewPatientViewModelWrongDataTestCases))]
        
        public void RegisterPatientShouldFailIfNullOrIncompleteData(PatientViewModel newPatient)
        {
            _pieService.Setup(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()));

            Task Act() => _manager.RegisterPatient(newPatient, It.IsAny<ClaimsPrincipal>());

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()), Times.Never);            
        }

        [Test]
        public void RegisterPatientWorksWithRightData()
        {
            PatientViewModel newPatient = new PatientViewModel()
            {
                MRN = "Sample",
                DateOfBirth = DateTime.Today,
                FirstName = "Sample",
                LastName = "Sample",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                }
            };

            _pieService.Setup(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()));

            Task Act() => _manager.RegisterPatient(newPatient, It.IsAny<ClaimsPrincipal>());

            _pieService.Verify(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()), Times.Once);
        }

        [Test]
        public void QuickPatientRegistrationWorks()
        {
            _pieService.Setup(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()));

            var result = _manager.QuickPatientRegistration(It.IsAny<ClaimsPrincipal>());

            _pieService.Verify(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()), Times.Once);
        }

        [Test, TestCaseSource(nameof(PatientUpdateViewModelWrongDataTestCases))]

        public void UpdatePatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            _pieService.Setup(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()));

            Task Act() => _manager.UpdatePatient(patient);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()), Times.Never);
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
                Sex = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                }
            };

            _pieService.Setup(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()));

            var result = _manager.UpdatePatient(patient);

            _pieService.Verify(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()), Times.Once);
        }

        [Test]
        public void DeleteWorks()
        {
            _pieService.Setup(mock => mock.DeletePatient(new Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest()));

            var result = _manager.DeletePatient(It.IsAny<ulong>());

            _pieService.Verify(mock => mock.DeletePatient(new Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest()), Times.Once);
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

            _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchResponse>());

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

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

            _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchResponse>());

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

            Assert.IsNotNull(response);
        }
    }
}
