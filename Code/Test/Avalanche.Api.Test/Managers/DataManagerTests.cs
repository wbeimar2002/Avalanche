using AutoMapper;

using Avalanche.Api.Managers.Data;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;

using Moq;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class DataManagerTests
    {
        Mock<IStorageService> _storageService;
        Mock<IDataManagementService> _dataManagementService;
        Mock<ISecurityService> _securityService;
        Mock<IAvidisService> _avidisService;
        Mock<IHttpContextAccessor> _httpContextAccessor;

        SetupConfiguration _setupConfiguration;

        IMapper _mapper;
        DataManager _manager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _dataManagementService = new Mock<IDataManagementService>();
            _securityService = new Mock<ISecurityService>();
            _avidisService = new Mock<IAvidisService>();

            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _setupConfiguration = new SetupConfiguration()
            {
                General = new GeneralSetupConfiguration()
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _manager = new DeviceDataManager(_mapper, _dataManagementService.Object, _storageService.Object, _httpContextAccessor.Object, _setupConfiguration, _securityService.Object, _avidisService.Object);
        }

        [Test]
        public void AddProcedureTypeShouldFailIfDepartmentIsNull()
        {
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
        public void DeleteProcedureTypeShouldFailIfDepartmentIsNull()
        {
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
        public void GetProceduresByDepartmentShouldFailIfDepartmentIsNull()
        {
            Task Act() => _manager.GetProcedureTypesByDepartment(null);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
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

            _dataManagementService.Setup(mock => mock.AddLabel(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddLabelRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.AddLabelResponse()
                {
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

            _dataManagementService.Setup(mock => mock.AddLabel(It.IsAny<Ism.Storage.DataManagement.Client.V1.Protos.AddLabelRequest>()))
                .ReturnsAsync(new Ism.Storage.DataManagement.Client.V1.Protos.AddLabelResponse()
                {
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

        [Test]
        public async Task DataManager_GetLabelWithIgnoreCustomExceptions_ReturnsEmptyLabel_Succeeds()
        {
            var request = new Ism.Storage.DataManagement.Client.V1.Protos.GetLabelRequest()
            {
                LabelName = "LabelA",
                ProcedureTypeId = null,
                IgnoreCustomExceptions = true
            };

            var response = new Ism.Storage.DataManagement.Client.V1.Protos.LabelMessage();

            //arrange
            _dataManagementService.Setup(mock => mock.GetLabel(request)).ReturnsAsync(response);

            //act
            var result = await _manager.GetLabel("labelA", null);

            //assert
            Assert.IsNull(result);
        }
    }
}
