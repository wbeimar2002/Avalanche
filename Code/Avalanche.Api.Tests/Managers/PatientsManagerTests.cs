﻿using AutoFixture;
using AutoMapper;
using Avalanche.Api.Managers.Health;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
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
        Mock<ISettingsService> _settingsService;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<IDataManagementService> _dataManagementService;

        IMapper _mapper;
        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _settingsService = new Mock<ISettingsService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _dataManagementService = new Mock<IDataManagementService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfigurations());
            });

            _mapper = config.CreateMapper();
            _manager = new PatientsManager(_pieService.Object, _settingsService.Object, _accessInfoFactory.Object, _mapper, _dataManagementService.Object);
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
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            _pieService.Setup(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()));

            Task Act() => _manager.RegisterPatient(newPatient, user);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.RegisterPatient(new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest()), Times.Never);            
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
                Department = new Department() { Id = 1, Name = "SampleDepartment" },
                Physician = new Physician()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureType() { Id = 1, Name = "SampleProcedureType" }
            };

            var response = new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse()
            { 
                PatientRecord = new Ism.Storage.Core.PatientList.V1.Protos.PatientRecordMessage()
                { 
                    InternalId = 1233
                }
            };

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.Core.DataManagement.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 0,
                    Id = 0,
                    Name = null
                });

            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>())).ReturnsAsync(response);

            var result = _manager.RegisterPatient(newPatient, user);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest>()), Times.Once);
            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>()), Times.Once);
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
                Department = new Department() { Id = 1, Name = "SampleDepartment" },
                Physician = new Physician()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureType() { Id = 1, Name = "SampleProcedureType" }
            };

            var response = new Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse()
            {
                PatientRecord = new Ism.Storage.Core.PatientList.V1.Protos.PatientRecordMessage()
                {
                    InternalId = 1233
                }
            };

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.Core.DataManagement.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 1,
                    Id = 1,
                    Name = "Existing"
                });

            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>())).ReturnsAsync(response);

            var result = _manager.RegisterPatient(newPatient, user);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest>()), Times.Never);
            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>()), Times.Once);
        }

        [Test]
        public void QuickPatientRegistrationWorks()
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            _pieService.Setup(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>()));

            var result = _manager.QuickPatientRegistration(user);

            _pieService.Verify(mock => mock.RegisterPatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>()), Times.Once);
        }

        [Test, TestCaseSource(nameof(PatientUpdateViewModelWrongDataTestCases))]

        public void UpdatePatientShouldFailIfNullOrIncompleteData(PatientViewModel patient)
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            _pieService.Setup(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()));
            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);

            Task Act() => _manager.UpdatePatient(patient, user);
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _pieService.Verify(mock => mock.UpdatePatient(new Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest()), Times.Never);
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
                Department = new Department() { Id = 1, Name = "SampleDepartment" },
                Physician = new Physician()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureType() { Id = 1, Name = "SampleProcedureType" }
            };

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.Core.DataManagement.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 0,
                    Id = 0,
                    Name = null
                });

            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);
            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>()));

            var result = _manager.UpdatePatient(existingPatient, user);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest>()), Times.Once);
            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>()), Times.Once);
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
                Department = new Department() 
                {
                    Id = 1,
                    Name = "SampleDepartment" 
                },
                Physician = new Physician()
                {
                    Id = "SampleId",
                    FirstName = "SampleFirstName",
                    LastName = "SampleLastName"
                },
                ProcedureType = new ProcedureType() 
                {
                    Id = 1,
                    Name = "SampleProcedureType" 
                }
            };

            var setupSettings = new Shared.Infrastructure.Models.SetupSettings()
            {
                Registration = new Shared.Infrastructure.Models.RegistrationSettings()
                {
                    Quick = new Shared.Infrastructure.Models.QuickRegistrationSettings()
                    {
                        DateFormat = "yyyyMMdd_T_mmss"
                    }
                }
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            _dataManagementService.Setup(mock => mock.GetProcedureType(It.IsAny< Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()))
                .ReturnsAsync(new Ism.Storage.Core.DataManagement.V1.Protos.ProcedureTypeMessage()
                {
                    DepartmentId = 1,
                    Id = 1,
                    Name = "Existing"
                });

            _settingsService.Setup(mock => mock.GetSetupSettings(It.IsAny<ConfigurationContext>())).ReturnsAsync(setupSettings);
            _pieService.Setup(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>()));

            var result = _manager.UpdatePatient(existingPatient, user);

            _dataManagementService.Verify(mock => mock.GetProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.GetProcedureTypeRequest>()), Times.Once);
            _dataManagementService.Verify(mock => mock.AddProcedureType(It.IsAny<Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest>()), Times.Never);
            _pieService.Verify(mock => mock.UpdatePatient(It.IsAny<Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>()), Times.Once);
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

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

            Assert.AreEqual(actionResult.Result.Count, filter.Limit);
            Assert.IsNotNull(actionResult);
        }

        [Test]
        public void ExecuteSearchDetailsShouldReturnResponse()
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

            var actionResult = _manager.Search(filter);

            _pieService.Verify(mock => mock.Search(It.IsAny<Ism.PatientInfoEngine.V1.Protos.SearchRequest>()), Times.Once);

            Assert.AreEqual(actionResult.Result.Count, filter.Limit); 
            Assert.IsNotNull(actionResult);
        }
    }
}
