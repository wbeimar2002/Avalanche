using AutoMapper;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using AvidisDeviceInterface.V1.Protos;
using Castle.Core.Configuration;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public partial class DeviceManagerTests
    {
        [Test]
        public void RouteVideoSourceShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.RouteVideoSource,
                Devices = new List<Device>() { new Device() { Id = "TP1" } },
                Destinations = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _routingService.Verify(mock => mock.RouteVideo(It.IsAny<Ism.Routing.V1.Protos.RouteVideoRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void UnrouteVideoSourceShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.UnrouteVideoSource,
                Devices = new List<Device>() { new Device() { Id = "TP1" } },
                Destinations = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _routingService.Verify(mock => mock.RouteVideo(It.IsAny<Ism.Routing.V1.Protos.RouteVideoRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void EnterFullScreenShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.EnterFullScreen,
                Devices = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _routingService.Verify(mock => mock.EnterFullScreen(It.IsAny<Ism.Routing.V1.Protos.EnterFullScreenRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExitFullScreenShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.ExitFullScreen,
                Devices = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _routingService.Verify(mock => mock.ExitFullScreen(It.IsAny<Ism.Routing.V1.Protos.ExitFullScreenRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ShowVideoRoutingPreviewHarwareModeShouldReturnResponse()
        {
            _settingsService.Setup(mock =>  mock.GetVideoRoutingSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(new RoutingSettings()
            {
                Mode = RoutingModes.Hardware
            });

            CommandViewModel commandViewModel = new CommandViewModel()
            {
                AdditionalInfo = "{\"X\":180.75,\"Y\":221,\"Width\":300,\"Height\":230.40625}",
                CommandType = Shared.Domain.Enumerations.CommandTypes.ShowVideoRoutingPreview,
                Devices = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _avidisService.Verify(mock => mock.ShowPreview(It.IsAny<ShowPreviewRequest>()), Times.Once);
            _avidisService.Verify(mock => mock.RoutePreview(It.IsAny<RoutePreviewRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ShowVideoRoutingPreviewSoftwareModeShouldReturnResponse()
        {
            _settingsService.Setup(mock => mock.GetVideoRoutingSettingsAsync(It.IsAny<ConfigurationContext>())).ReturnsAsync(new RoutingSettings()
            {
                Mode = RoutingModes.Software
            });

            CommandViewModel commandViewModel = new CommandViewModel()
            {
                AdditionalInfo = "{\"X\":180.75,\"Y\":221,\"Width\":300,\"Height\":230.40625}",
                CommandType = Shared.Domain.Enumerations.CommandTypes.ShowVideoRoutingPreview,
                Devices = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _avidisService.Verify(mock => mock.RoutePreview(It.IsAny<RoutePreviewRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void HideVideoRoutingPreviewShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.HideVideoRoutingPreview,
                Devices = new List<Device>() { new Device() { Id = "TP1" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _avidisService.Verify(mock => mock.HidePreview(It.IsAny<HidePreviewRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
