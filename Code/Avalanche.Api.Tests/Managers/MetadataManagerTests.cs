using AutoMapper;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
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
        Mock<ISettingsService> _settingsService;
        Mock<IDataManagementService> _dataManagementService;

        IMapper _mapper;
        MetadataManager _manager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _settingsService = new Mock<ISettingsService>();
            _dataManagementService = new Mock<IDataManagementService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfigurations());
            });

            _mapper = config.CreateMapper();
            _manager = new MetadataManager(_storageService.Object, _dataManagementService.Object, _settingsService.Object, _mapper);
        }

        [Test]
        public void AddProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = false
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            var newProcedureType = new ProcedureType()
            {
                Name = "Sample",
                Department = "Invalid Department"
            };

            Task Act() => _manager.AddProcedureType(It.IsAny<User>(), newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = true
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            var newProcedureType = new ProcedureType()
            {
                Name = "Sample",
                Department = null
            };

            Task Act() => _manager.AddProcedureType(It.IsAny<User>(), newProcedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = false
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            var procedureType = new ProcedureType()
            {
                Name = "Sample",
                Department = "Invalid Department"
            };

            Task Act() => _manager.DeleteProcedureType(It.IsAny<User>(), procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteProcedureTypeShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = true
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            var procedureType = new ProcedureType()
            {
                Name = "Sample",
                Department = null
            };

            Task Act() => _manager.DeleteProcedureType(It.IsAny<User>(), procedureType);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfHasDepartmentAndDepartmentIsNotSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = false
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            Task Act() => _manager.GetProceduresByDepartment(It.IsAny<User>(), "Sample Invalid Department");

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = true
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            Task Act() => _manager.GetProceduresByDepartment(It.IsAny<User>(), null);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = false
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            Task Act() => _manager.AddDepartment(It.IsAny<User>(), It.IsAny<Department>());

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void DeleteDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var settingsDepartmentNotSupported = new SetupSettings()
            {
                DepartmentsSupported = false
            };

            _settingsService.Setup(mock => mock.GetSetupSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmentNotSupported);

            Task Act() => _manager.DeleteDepartment(It.IsAny<User>(), "Sample Department");

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }
    }
}
