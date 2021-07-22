using AutoMapper;
using Avalanche.Api.Managers.Presets;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models.Presets;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class PresetManagerTests
    {
        IMapper _mapper;
        Mock<IRoutingService> _routingService;
        Mock<IStorageService> _storageService;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RoutingMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _routingService = new Mock<IRoutingService>();
            _storageService = new Mock<IStorageService>();
        }

        [Test]
        public async Task TestGetPresetsValid()
        {
            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = new UserPresetsModel();

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);
            var presets = await manager.GetPresets("1");

            Assert.NotNull(presets);
        }

        [Test]
        public void TestGetPresetsInvalid()
        {
            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = new UserPresetsModel();

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await manager.GetPresets("2"));
        }


        [Test]
        public async Task TestSavePresets_SaveCalled()
        {
            _routingService.Setup(r => r.GetCurrentRoutes())
                .ReturnsAsync(new Ism.Routing.V1.Protos.GetCurrentRoutesResponse { 
                
                });

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);
            await manager.SavePreset("1", 1, "TestPreset");

            _storageService.Verify(s => s.SaveJsonMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()), Times.Once);
        }

        [Test]
        public async Task TestApplyPresets_ValidPreset()
        {
            var userPresets = new UserPresetsModel();
            userPresets.RoutingPresets[1] = new RoutingPresetModel { Id = 1, Name="Preset1"};

            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = userPresets;

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);
            await manager.ApplyPreset("1", 1);

            _routingService.Verify(s => s.RouteVideoBatch(It.IsAny<RouteVideoBatchRequest>()), Times.Once);
        }

        [Test]
        public void TestApplyPresets_InvalidPreset()
        {
            var userPresets = new UserPresetsModel();
            userPresets.RoutingPresets[1] = new RoutingPresetModel { Id = 1, Name = "Preset1" };

            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = userPresets;

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await manager.ApplyPreset("2", 1));
        }

        [Test]
        public async Task TestRemovePresets_ValidPreset()
        {
            var userPresets = new UserPresetsModel();
            userPresets.RoutingPresets[1] = new RoutingPresetModel { Id = 1, Name = "Preset1" };

            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = userPresets;

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);
            await manager.RemovePreset("1", 1);
        }

        [Test]
        public void TestRemovePresets_InvalidPreset()
        {
            var userPresets = new UserPresetsModel();
            userPresets.RoutingPresets[1] = new RoutingPresetModel { Id = 1, Name = "Preset1" };

            var samplePreset = new PresetsModel();
            samplePreset.Users["1"] = userPresets;

            _storageService.Setup(r => r.GetJsonObject<PresetsModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(samplePreset);

            var manager = new PresetManager(_routingService.Object, _storageService.Object, _mapper);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await manager.RemovePreset("2", 1));
        }
    }
}
