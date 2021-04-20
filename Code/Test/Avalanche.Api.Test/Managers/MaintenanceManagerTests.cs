using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class MaintenanceManagerTests
    {
        IMapper _mapper;

        Mock<ILibraryService> _libraryService;
        Mock<IStorageService> _storageService;
        Mock<IDataManager> _dataManager;
        Mock<IHttpContextAccessor> _httpContextAccessor;


        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfiguration());
                cfg.AddProfile(new MaintenanceMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _libraryService = new Mock<ILibraryService>();
            _storageService = new Mock<IStorageService>();
            _dataManager = new Mock<IDataManager>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Test]
        public async Task TestReindexRepositoryReturnsStatus()
        {
            var expected = new Ism.Library.V1.Protos.ReindexRepositoryResponse { ErrorCount = 2, SuccessCount = 4, TotalCount = 6 };
            _libraryService.Setup(m => m.ReindexRepository(It.IsAny<string>())).ReturnsAsync(expected);
            var manager = new MaintenanceManager(_storageService.Object, _dataManager.Object, _mapper, _httpContextAccessor.Object, _libraryService.Object);

            var response = await manager.ReindexRepository(new ViewModels.ReindexRepositoryRequestViewModel("repo"));

            Assert.NotNull(response);
            Assert.AreEqual(expected.ErrorCount, response.ErrorCount);
            Assert.AreEqual(expected.SuccessCount, response.SuccessCount);
            Assert.AreEqual(expected.TotalCount, response.TotalCount);
        }
    }
}
