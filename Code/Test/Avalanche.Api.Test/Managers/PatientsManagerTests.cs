using AutoMapper;
using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.PatientInfoEngine.V1.Protos;

using Microsoft.AspNetCore.Http;

using Moq;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{

    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<IHttpContextAccessor> _httpContextAccessor;
        Mock<ISecurityService> _securityService;
        SetupConfiguration _setupConfiguration;

        IMapper _mapper;
        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _securityService = new Mock<ISecurityService>();
            _setupConfiguration = new SetupConfiguration()
            {
                General = new GeneralSetupConfiguration()
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
                cfg.AddProfile(new ProceduresMappingConfiguration());
            });

            var recorderConfig = new RecorderConfiguration { BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately };

            _accessInfoFactory.Setup(m => m.GenerateAccessInfo(It.IsAny<string>()))
                .Returns(new Ism.IsmLogCommon.Core.AccessInfo("127.0.0.1", "tests", "unit-tests", "name", "none", false));

            _mapper = config.CreateMapper();

            _manager = new PatientsManager(_pieService.Object,
                _accessInfoFactory.Object,
                _mapper,
                _httpContextAccessor.Object,
                _setupConfiguration,
                _securityService.Object);
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
                    FirstName = "Sample",
                    LastName = null,
                    Sex = new KeyValuePairViewModel()
                    {
                        Id = "S",
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
                    FirstName = "Sample",
                    LastName = null,
                    Sex = new KeyValuePairViewModel()
                    {
                        Id = "S",
                        TranslationKey = "SampleKey",
                        Value = "Sample"
                    }
                });
            }
        }

        //[Test, TestCaseSource(nameof(NewPatientViewModelWrongDataTestCases))]
        //public void RegisterPatientShouldFailIfNullOrIncompleteData(PatientViewModel newPatient)
        //{
        //    _pieService.Setup(mock => mock.RegisterPatient(new AddPatientRecordRequest()));

        //    Task Act() => _manager.RegisterPatient(newPatient);

        //    Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

        //    _pieService.Verify(mock => mock.RegisterPatient(new AddPatientRecordRequest()), Times.Never);
        //}

        //[Test]
        //public async Task QuickPatientRegistrationWorks()
        //{
        //    _setupConfiguration.Registration = new RegistrationSetupConfiguration
        //    {
        //        Quick = new QuickSetupConfiguration
        //        {
        //            DateFormat = "yyyyMMdd_T_mmss"
        //        }
        //    };

        //    _setupConfiguration.PatientInfo = new List<PatientInfoSetupConfiguration>();
        //    var result = await _manager.QuickPatientRegistration();

        //    var faker = new Faker();
        //    _activeProcedureManager.Setup(m => m.AllocateNewProcedure(PatientRegistrationMode.Quick, null))
        //    .ReturnsAsync(new ProcedureAllocationViewModel(new ProcedureIdViewModel(Guid.NewGuid().ToString(), faker.Commerce.Department()), faker.System.FilePath()));

        //    _activeProcedureManager.Verify(m => m.AllocateNewProcedure(PatientRegistrationMode.Quick, result), Times.Never);
        //}

        [Test, TestCaseSource(nameof(PatientUpdateViewModelWrongDataTestCases))]

        public void UpdatePatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            _setupConfiguration = new SetupConfiguration
            {
                Registration = new RegistrationSetupConfiguration
                {
                    Quick = new QuickSetupConfiguration
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _pieService.Setup(mock => mock.UpdatePatient(new UpdatePatientRecordRequest()));

            Task Act() => _manager.UpdatePatient(patient);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.UpdatePatient(new UpdatePatientRecordRequest()), Times.Never);
        }

        [Test]
        public async Task DeleteWorks()
        {
            _pieService.Setup(mock => mock.DeletePatient(It.IsAny<DeletePatientRecordRequest>()));

            await _manager.DeletePatient(It.IsAny<ulong>());

            _pieService.Verify(mock => mock.DeletePatient(It.IsAny<DeletePatientRecordRequest>()), Times.Once);
        }

        //TODO: Pending to solve
        //[Test]
        //public async Task ExecuteSearchKeywordShouldReturnResponse()
        //{
        //    PatientKeywordSearchFilterViewModel filter = new PatientKeywordSearchFilterViewModel()
        //    {
        //        Limit = 10,
        //        Page = 0,
        //        Term = "name"
        //    };

        //    var cultureName = CultureInfo.CurrentCulture.Name;
        //    cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

        //    Fixture autoFixture = new Fixture();
        //    var patientRecords = autoFixture.CreateMany<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>(filter.Limit);

        //    var serviceResponse = new Ism.PatientInfoEngine.V1.Protos.SearchResponse();
        //    serviceResponse.UpdatedPatList.Add(patientRecords);

        //    foreach (var item in serviceResponse.UpdatedPatList)
        //    {
        //        item.Patient.Dob = new Ism.PatientInfoEngine.V1.Protos.FixedDateMessage()
        //        {
        //            Year = DateTime.Now.Year,
        //            Month = DateTime.Now.Month,
        //            Day = DateTime.Now.Day
        //        };
        //    }

        //    _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(serviceResponse);

        //    var actionResult = await _manager.Search(filter);

        //    _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

        //    Assert.AreEqual(actionResult.Count, filter.Limit);
        //    Assert.IsNotNull(actionResult);
        //}

        //TODO: Pending to solve
        //[Test]
        //public async Task ExecuteSearchDetailsShouldReturnResponse()
        //{
        //    PatientDetailsSearchFilterViewModel filter = new PatientDetailsSearchFilterViewModel()
        //    {
        //        Limit = 10,
        //        Page = 0,
        //        RoomName = "Room",
        //    };

        //    var cultureName = CultureInfo.CurrentCulture.Name;
        //    cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

        //    Fixture autoFixture = new Fixture();
        //    var patientRecords = autoFixture.CreateMany<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>(filter.Limit);

        //    var serviceResponse = new Ism.PatientInfoEngine.V1.Protos.SearchResponse();
        //    serviceResponse.UpdatedPatList.Add(patientRecords);

        //    foreach (var item in serviceResponse.UpdatedPatList)
        //    {
        //        item.Patient.Dob = new Ism.PatientInfoEngine.V1.Protos.FixedDateMessage()
        //        {
        //            Year = DateTime.Now.Year,
        //            Month = DateTime.Now.Month,
        //            Day = DateTime.Now.Day
        //        };
        //    }

        //    _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(serviceResponse);

        //    var actionResult = await _manager.Search(filter);

        //    _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

        //    Assert.AreEqual(actionResult.Count, filter.Limit); 
        //    Assert.IsNotNull(actionResult);
        //}
    }
}
