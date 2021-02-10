using AutoFixture;
using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Common.Core.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
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
                cfg.AddProfile(new HealthMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _manager = new DataManager(_storageService.Object, _dataManagementService.Object, _mapper, _httpContextAccessor.Object);
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

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData",1,  It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var newProcedureType = new ProcedureType()
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
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var newProcedureType = new ProcedureType()
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
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var procedureType = new ProcedureType()
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
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            var procedureType = new ProcedureType()
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
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.GetProcedureTypesByDepartment(1);

            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetProceduresByDepartmentShouldFailIfDepartmentIsNullAndDepartmentIsSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = true
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.GetProcedureTypesByDepartment(null);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.AddDepartment(It.IsAny<Department>());

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void DeleteDepartmentShouldFailIfDepartmentIsNotSupported()
        {
            var settingsDepartmenSupported = new
            {
                General = new
                {
                    DepartmentsSupported = false
                }
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("SetupSettingsData", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(settingsDepartmenSupported);

            Task Act() => _manager.DeleteDepartment(1);

            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }
    }
}
