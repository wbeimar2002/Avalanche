using AutoFixture;
using AutoMapper;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Common.Core.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class MetadataManagerTests
    {
        Mock<IStorageService> _storageService;
        Mock<IDataManagementService> _dataManagementService;

        IMapper _mapper;
        MetadataManager _manager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _dataManagementService = new Mock<IDataManagementService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _manager = new MetadataManager(_storageService.Object, _dataManagementService.Object, _mapper);
        }

        [Test]
        public void AddProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var settingsDepartmenSupported = new 
            {
                General = new 
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues",1,  It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var newProcedureType = new ProcedureType()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = 1
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            Task Act() => _manager.AddProcedureType(user, newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var newProcedureType = new ProcedureType()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = null
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            Task Act() => _manager.AddProcedureType(user, newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var procedureType = new ProcedureType()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = 1
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            Task Act() => _manager.DeleteProcedureType(user, procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var procedureType = new ProcedureType()
            {
                Id = 1,
                Name = "Sample",
                DepartmentId = null
            };

            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            Task Act() => _manager.DeleteProcedureType(user, procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.GetProcedureTypesByDepartment(user, 1);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.GetProcedureTypesByDepartment(user, null);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.AddDepartment(user, It.IsAny<Department>());

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void DeleteDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            Fixture fixture = new Fixture();
            var user = fixture.Create<User>();

            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.DeleteDepartment(user, 1);

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }
    }
}
