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
using Castle.Core.Configuration;
using Google.Protobuf.WellKnownTypes;
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

            _routingService.Setup(mock => mock.RouteVideo(It.IsAny<Ism.Routing.V1.Protos.RouteVideoRequest>()));

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
    }
}
