using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Helpers;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Maintenance;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Printing;
using Avalanche.Api.ViewModels;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

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
        Mock<IFilesService> _filesService;
        Mock<IPrintingService> _printingService;

        Mock<ISharedConfigurationManager> _sharedConfigurationManager;
        Mock<IConfigurationManager> _deviceConfigurationManager;

        MaintenanceManager _manager;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _libraryService = new Mock<ILibraryService>();
            _storageService = new Mock<IStorageService>();
            _dataManager = new Mock<IDataManager>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _filesService = new Mock<IFilesService>();
            _printingService = new Mock<IPrintingService>();
            _sharedConfigurationManager = new Mock<ISharedConfigurationManager>();
            _deviceConfigurationManager = new Mock<IConfigurationManager>();

            _manager = new DeviceMaintenanceManager(
                _storageService.Object,
                _dataManager.Object,
                _mapper,
                _httpContextAccessor.Object,
                _libraryService.Object,
                _filesService.Object,
                _printingService.Object,
                _sharedConfigurationManager.Object, _deviceConfigurationManager.Object);
        }

        [Test]
        public async Task TestSaveCategoryPolicies_CallsVerify()
        {
            DynamicSectionViewModel category = new DynamicSectionViewModel()
            {
                Metadata = "SampleMetadata",
                Schema = "SchemaMetadata",
                JsonKey = "Sample"
            };

            string json = DynamicSettingsHelper.GetJsonValues(category);
            _storageService.Setup(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(true);

            await _manager.SaveCategoryPolicies(category);

            _storageService.Verify(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(category.JsonKey, json, 1, It.IsAny<ConfigurationContext>(), false), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonMetadata(category.Metadata, JsonConvert.SerializeObject(category), 1, It.IsAny<ConfigurationContext>()), Times.Once);
        }

        [Test]
        public async Task TestSaveCategoryPolicies_CallsVerify_WithBadSchema()
        {
            DynamicSectionViewModel category = new DynamicSectionViewModel()
            {
                Metadata = "SampleMetadata",
                Schema = "SchemaMetadata",
                JsonKey = "Sample"
            };

            string json = DynamicSettingsHelper.GetJsonValues(category);
            _storageService.Setup(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<ValidationException>(() => _manager.SaveCategoryPolicies(category));

            _storageService.Verify(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(category.JsonKey, json, 1, It.IsAny<ConfigurationContext>(), false), Times.Never);
            _storageService.Verify(mock => mock.SaveJsonMetadata(category.Metadata, JsonConvert.SerializeObject(category), 1, It.IsAny<ConfigurationContext>()), Times.Never);
        }

        [Test]
        public async Task TestSaveCategory_CallsVerify()
        {
            DynamicSectionViewModel category = new DynamicSectionViewModel()
            {
                Metadata = "SampleMetadata",
                Schema = "SchemaMetadata",
                JsonKey = "Sample"
            };

            string json = DynamicSettingsHelper.GetJsonValues(category);
            _storageService.Setup(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(true);

            await _manager.SaveCategory(category);

            _storageService.Verify(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(category.JsonKey, json, 1, It.IsAny<ConfigurationContext>(), false), Times.Once);
        }

        [Test]
        public async Task TestSaveCategory_CallsVerify_WithBadSchema()
        {
            DynamicSectionViewModel category = new DynamicSectionViewModel()
            {
                Metadata = "SampleMetadata",
                Schema = "SchemaMetadata",
                JsonKey = "Sample"
            };

            string json = DynamicSettingsHelper.GetJsonValues(category);
            _storageService.Setup(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<ValidationException>(() => _manager.SaveCategory(category));

            _storageService.Verify(mock => mock.ValidateSchema(category.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(category.JsonKey, json, 1, It.IsAny<ConfigurationContext>(), false), Times.Never);
        }

        [Test]
        public async Task TestSaveEntityChangesSaveAsFile_CallsVerify()
        {
            DynamicListViewModel customList = new DynamicListViewModel()
            {
                Schema = "SchemaMetadata",
                SaveAsFile = true,
                SourceKey = "SampleList"
            };

            var json = JsonConvert.SerializeObject(customList.Data);
            _storageService.Setup(mock => mock.ValidateSchema(customList.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(true);

            await _manager.SaveEntityChanges(customList, It.IsAny<Shared.Infrastructure.Enumerations.DynamicListActions>());

            _storageService.Verify(mock => mock.ValidateSchema(customList.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(customList.SourceKey, json, 1, It.IsAny<ConfigurationContext>(), true), Times.Once);
        }

        [Test]
        public async Task TestSaveEntityChangesSaveAsFile_CallsVerify_WithBadSchema()
        {
            DynamicListViewModel customList = new DynamicListViewModel()
            {
                Schema = "SchemaMetadata",
                SaveAsFile = true,
                SourceKey = "SampleList"
            };

            var json = JsonConvert.SerializeObject(customList.Data);
            _storageService.Setup(mock => mock.ValidateSchema(customList.Schema, json, 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<ValidationException>(() => _manager.SaveEntityChanges(customList, It.IsAny<Shared.Infrastructure.Enumerations.DynamicListActions>()));

            _storageService.Verify(mock => mock.ValidateSchema(customList.Schema, json, 1, It.IsAny<ConfigurationContext>()), Times.Once);
            _storageService.Verify(mock => mock.SaveJsonObject(customList.SourceKey, json, 1, It.IsAny<ConfigurationContext>(), true), Times.Never);
        }
    }
}
