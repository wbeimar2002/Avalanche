using AutoMapper;

using Avalanche.Api.Managers.Data;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

using Moq;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class DataManagerTests
    {
        Mock<IStorageService> _storageService;
        Mock<IDataManagementService> _dataManagementService;
        Mock<IHttpContextAccessor> _httpContextAccessor;

        IMapper _mapper;
        DataManager _manager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _dataManagementService = new Mock<IDataManagementService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _manager = new DataManager(_storageService.Object, _dataManagementService.Object, _mapper, _httpContextAccessor.Object);
        }

        [Test]
        public void AddProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration 
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1,  It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            var newProcedureType = new ProcedureTypeModel()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = 1
            };

            Task Act() => _manager.AddProcedureType(newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            var newProcedureType = new ProcedureTypeModel()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = null
            };

            Task Act() => _manager.AddProcedureType(newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration 
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            var procedureType = new ProcedureTypeModel()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = 1
            };

            Task Act() => _manager.DeleteProcedureType(procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            var procedureType = new ProcedureTypeModel()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = null
            };

            Task Act() => _manager.DeleteProcedureType(procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            Task Act() => _manager.GetProcedureTypesByDepartment(1);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            Task Act() => _manager.GetProcedureTypesByDepartment(null);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            Task Act() => _manager.AddDepartment(It.IsAny<DepartmentModel>());

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void DeleteDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            Task Act() => _manager.DeleteDepartment(1);

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task AddLabelWithValidLabelName()
        {
            var newLabel = new LabelModel()
            {
                Id = 1,
                Name = "Sample",
                ProcedureTypeId = 1
            };
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            _dataManagementService.Setup(mock => mock.AddLabel(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddLabelRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.AddLabelResponse()
                {
                    IsNew = true,
                    Label = new Ism.Storage.DataManagement.Client.V1.Protos.LabelMessage
                    {
                        Id = 1,
                        Name = "Sample",
                        ProcedureTypeId = 1
                    }
                });

            var result = await _manager.AddLabel(newLabel);
            Assert.AreEqual(result.Name, newLabel.Name);
        }

        [Test]
        public void AddLabelWithLabelNameAsNull()
        {
            var newLabel = new LabelModel()
            {
                Id = 1,
                Name = null,
                ProcedureTypeId = 1
            };
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            Task Act = _manager.AddLabel(newLabel);
            Assert.AreEqual(Act.Exception.InnerException.Message, "Value cannot be null. (Parameter 'Name')");
        }

        [Test]
        public async Task AddLabelWithValidLabelNameAndEmptyProcedureType()
        {
            var newLabel = new LabelModel()
            {
                Id = 1,
                Name = "Sample",
                ProcedureTypeId = null
            };
            var setupConfiguration = new SetupConfiguration
            {
                General = new GeneralSetupConfiguration
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(setupConfiguration);

            _dataManagementService.Setup(mock => mock.AddLabel(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddLabelRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.AddLabelResponse()
                {
                    IsNew = true,
                    Label = new Ism.Storage.DataManagement.Client.V1.Protos.LabelMessage
                    {
                        Id = 1,
                        Name = "Sample",
                        ProcedureTypeId = null
                    }
                });

            var result = await _manager.AddLabel(newLabel);
            Assert.AreEqual(result.Name, newLabel.Name);
            Assert.AreEqual(result.ProcedureTypeId, null);
        }

        [Test]
        public async Task DeleteLabelWithValidLabelName()
        {
            var newLabel = new LabelModel()
            {
                Id = 1,
                Name = "Sample",
                ProcedureTypeId = 1
            };

            _dataManagementService.Setup(mock => mock.DeleteLabel(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.DeleteLabelRequest>())).Returns(Task.CompletedTask);

            await _manager.DeleteLabel(newLabel);

            _dataManagementService
                .Verify(mock => mock.DeleteLabel(_mapper.Map<LabelModel, Ism.Storage.DataManagement.Client.V1.Protos.DeleteLabelRequest>(newLabel)), Times.Once);
        }

        [Test]
        public async Task GetLabelsByProcedureTypeWithValidProcedureTypeId()
        {
            var newLabel = new LabelModel()
            {
                Id = 1,
                Name = "Sample",
                ProcedureTypeId = 1
            };

            var response = new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelsResponse();            

            _dataManagementService.Setup(mock => mock.GetLabelsByProcedureType(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.GetLabelsByProcedureTypeRequest>()))
                .ReturnsAsync(response);

            var result = await _manager.GetLabelsByProcedureType(newLabel.ProcedureTypeId);

            _dataManagementService.Verify(mock => mock.GetLabelsByProcedureType(
                new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelsByProcedureTypeRequest()
                {
                    ProcedureTypeId = newLabel.ProcedureTypeId
                }), Times.Once); 
        }

        [Test]
        public async Task GetAllLabels()
        {
            var response = new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelsResponse();

            _dataManagementService.Setup(mock => mock.GetAllLabels()).ReturnsAsync(response);

            var result = await _manager.GetAllLabels();

            _dataManagementService.Verify(mock => mock.GetAllLabels(), Times.Once);
        }
    }
}
