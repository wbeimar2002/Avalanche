using AutoFixture;

using AutoMapper;

using Avalanche.Api.Managers.Patients;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;

using Bogus;

using Ism.Common.Core.Configuration.Models;
using Ism.Storage.PatientList.Client.V1.Protos;
using Ism.SystemState.Client;

using Microsoft.AspNetCore.Http;

using Moq;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{

    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<IStorageService> _storageService;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<IDataManagementService> _dataManagementService;
        Mock<IStateClient> _stateClient;
        Mock<IHttpContextAccessor> _httpContextAccessor;
        Mock<IProceduresManager> _proceduresManager;

        IMapper _mapper;
        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _storageService = new Mock<IStorageService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _dataManagementService = new Mock<IDataManagementService>();
            _stateClient = new Mock<IStateClient>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _proceduresManager = new Mock<IProceduresManager>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfiguration());
                cfg.AddProfile(new ProceduresMappingConfiguration());
            });

            _accessInfoFactory.Setup(m => m.GenerateAccessInfo(It.IsAny<string>()))
                .Returns(new Ism.IsmLogCommon.Core.AccessInfo("127.0.0.1", "tests", "unit-tests", "name", "none", false));

            _mapper = config.CreateMapper();
            _manager = new PatientsManager(_pieService.Object, _accessInfoFactory.Object, _storageService.Object, _mapper, _dataManagementService.Object, _stateClient.Object, _proceduresManager.Object, _httpContextAccessor.Object);
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
            _pieService.Setup(mock => mock.RegisterPatient(new Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest()));

            Task Act() => _manager.RegisterPatient(newPatient);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.RegisterPatient(new Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest()), Times.Never);            
        }

        [Test]
        public void RegisterPatientWorksWithRightDataShouldAddProcedureTypeIfNotExists()
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
                },
                Department = new DepartmentModel() { Id = 1, Name = "SampleDepartment" },
                Physician = new PhysicianModel()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureTypeModel() { Id = 1, Name = "SampleProcedureType" }
            };

            var response = new Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse()
            { 
                PatientRecord = new Ism.Storage.PatientList.Client.V1.Protos.PatientRecordMessage()
                { 
                    InternalId = 1233
                }
            };

            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 0,
                    Id = 0,
                    Name = null
                });

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>())).ReturnsAsync(response);

            var result = _manager.RegisterPatient(newPatient);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddProcedureTypeRequest>()), Times.Once);
            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>()), Times.Once);
        }

        [Test]
        public void RegisterPatientWorksWithRightDataShouldNotAddProcedureTypeIfExists()
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
                },
                Department = new DepartmentModel() { Id = 1, Name = "SampleDepartment" },
                Physician = new PhysicianModel()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureTypeModel() { Id = 1, Name = "SampleProcedureType" }
            };

            var response = new Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse()
            {
                PatientRecord = new Ism.Storage.PatientList.Client.V1.Protos.PatientRecordMessage()
                {
                    InternalId = 1233
                }
            };

            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 1,
                    Id = 1,
                    Name = "Existing"
                });

            var faker = new Faker();
            _proceduresManager.Setup(m => m.AllocateNewProcedure())
                .ReturnsAsync(new ProcedureAllocationViewModel(new ProcedureIdViewModel(Guid.NewGuid().ToString(), faker.Commerce.Department()), faker.System.FilePath()));

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>())).ReturnsAsync(response);

            var result = _manager.RegisterPatient(newPatient);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddProcedureTypeRequest>()), Times.Never);
            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>()), Times.Once);
        }

        [Test]
        public async Task QuickPatientRegistrationWorks()
        {
            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            var faker = new Faker();
            _proceduresManager.Setup(m => m.AllocateNewProcedure())
                .ReturnsAsync(new ProcedureAllocationViewModel(new ProcedureIdViewModel(Guid.NewGuid().ToString(), faker.Commerce.Department()), faker.System.FilePath()));

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>()));

            var result = await _manager.QuickPatientRegistration();

            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(PatientUpdateViewModelWrongDataTestCases))]

        public void UpdatePatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _pieService.Setup(mock => mock.UpdatePatient(new Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest()));
            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            Task Act() => _manager.UpdatePatient(patient);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.UpdatePatient(new Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest()), Times.Never);
        }

        [Test]

        public void UpdatePatientWorksWithRightDataShouldAddProcedureTypeIfNotExists()
        {
            PatientViewModel existingPatient = new PatientViewModel()
            {
                Id = 1,
                MRN = "Sample",
                DateOfBirth = DateTime.Today,
                FirstName = "Sample",
                LastName = "Sample",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                },
                Department = new DepartmentModel() { Id = 1, Name = "SampleDepartment" },
                Physician = new PhysicianModel()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureTypeModel() { Id = 1, Name = "SampleProcedureType" }
            };

            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 0,
                    Id = 0,
                    Name = null
                });

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);
            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest>()));

            var result = _manager.UpdatePatient(existingPatient);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddProcedureTypeRequest>()), Times.Once);
            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest>()), Times.Once);
        }

        [Test]

        public void UpdatePatientWorksWithRightDataShouldNotAddProcedureTypeIfExists()
        {
            PatientViewModel existingPatient = new PatientViewModel()
            {
                Id = 1,
                MRN = "Sample",
                DateOfBirth = DateTime.Today,
                FirstName = "Sample",
                LastName = "Sample",
                Sex = new KeyValuePairViewModel()
                {
                    Id = "S",
                    TranslationKey = "SampleKey",
                    Value = "Sample"
                },
                Department = new DepartmentModel() 
                {
                    Id = 1,
                    Name = "SampleDepartment" 
                },
                Physician = new PhysicianModel()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureTypeModel() 
                {
                    Id = 1,
                    Name = "SampleProcedureType" 
                }
            };

            var setupSettings = new 
            {
                Registration = new 
                {
                    Quick = new 
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny< Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 1,
                    Id = 1,
                    Name = "Existing"
                });

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);
            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<UpdatePatientRecordRequest>()));

            var result = _manager.UpdatePatient(existingPatient);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddProcedureTypeRequest>()), Times.Never);
            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<UpdatePatientRecordRequest>()), Times.Once);
        }

        [Test]
        public async Task DeleteWorks()
        {
            _pieService.Setup(mock => mock.DeletePatient(It.IsAny<DeletePatientRecordRequest>()));

            await _manager.DeletePatient(It.IsAny<ulong>());

            _pieService.Verify(mock => mock.DeletePatient(It.IsAny<DeletePatientRecordRequest>()), Times.Once);
        }


        [Test]
        public async Task ExecuteSearchKeywordShouldReturnResponse()
        {
            PatientKeywordSearchFilterViewModel filter = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                Term = "name"
            };

            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            Fixture autoFixture = new Fixture();
            var patientRecords = autoFixture.CreateMany<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>(filter.Limit);

            var serviceResponse = new Ism.PatientInfoEngine.V1.Protos.SearchResponse();
            serviceResponse.UpdatedPatList.Add(patientRecords);

            foreach (var item in serviceResponse.UpdatedPatList)
            {
                item.Patient.Dob = new Ism.PatientInfoEngine.V1.Protos.FixedDateMessage()
                {
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    Day = DateTime.Now.Day
                };
            }

            _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(serviceResponse);

            var actionResult = await _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

            Assert.AreEqual(actionResult.Count, filter.Limit);
            Assert.IsNotNull(actionResult);
        }

        [Test]
        public async Task ExecuteSearchDetailsShouldReturnResponse()
        {
            PatientDetailsSearchFilterViewModel filter = new PatientDetailsSearchFilterViewModel()
            {
                Limit = 10,
                Page = 0,
                RoomName = "Room",
            };

            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            Fixture autoFixture = new Fixture();
            var patientRecords = autoFixture.CreateMany<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage>(filter.Limit);

            var serviceResponse = new Ism.PatientInfoEngine.V1.Protos.SearchResponse();
            serviceResponse.UpdatedPatList.Add(patientRecords);

            foreach (var item in serviceResponse.UpdatedPatList)
            {
                item.Patient.Dob = new Ism.PatientInfoEngine.V1.Protos.FixedDateMessage()
                {
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    Day = DateTime.Now.Day
                };
            }

            _pieService.Setup(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>())).ReturnsAsync(serviceResponse);

            var actionResult = await _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

            Assert.AreEqual(actionResult.Count, filter.Limit); 
            Assert.IsNotNull(actionResult);
        }
    }
}
