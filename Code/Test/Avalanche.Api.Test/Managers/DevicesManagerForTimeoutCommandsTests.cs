using AutoMapper;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
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
        public void TimeoutExecutePlaySlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutPlayPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.IsAny<Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModePgsIfAlwaysOnIsTrue()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutStopPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var pgsSettings = new 
            {
                PgsVideoAlwaysOn = true
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("PgsSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(pgsSettings);

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.Is<Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>(args => (int)args.Mode == (int)PgsTimeoutModes.Pgs)), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModeIdleIfAlwaysOnIsFalse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutStopPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var pgsSettings = new 
            {
                PgsVideoAlwaysOn = false
            };

            _storageService.Setup(mock => mock.GetJsonDynamic("PgsSettingsValues", 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(pgsSettings);

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.Is<Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>(args => (int)args.Mode == (int)PgsTimeoutModes.Idle)), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteNextSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutNextPdfSlide,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };
            
            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.NextPageAsync(), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecutePreviousSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutPreviousPdfSlide,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.PreviousPageAsync(), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteSetCurrentPageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.TimeoutSetCurrentSlide,
                Devices = new List<Device>() { new Device() { Id = "Testing" } },
                Message = "0"
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetTimeoutPageAsync(It.IsAny<Ism.PgsTimeout.V1.Protos.SetTimeoutPageRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
