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
        public void TimeoutExecutePlaySlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPlayPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModePgsIfAlwaysOnIsTrue()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            TimeoutSettings timeoutSettings = new TimeoutSettings()
            {
                PgsVideoAlwaysOn = true
            };

            _settingsService.Setup(mock => mock.GetTimeoutSettingsAsync()).ReturnsAsync(timeoutSettings);

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.Is<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>(args => (int)args.Mode == (int)TimeoutModes.Pgs)), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModeIdleIfAlwaysOnIsFalse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            TimeoutSettings timeoutSettings = new TimeoutSettings()
            {
                PgsVideoAlwaysOn = false
            };

            _settingsService.Setup(mock => mock.GetTimeoutSettingsAsync()).ReturnsAsync(timeoutSettings);

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.Is<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>(args => (int)args.Mode == (int)TimeoutModes.Idle)), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteNextSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutNextPdfSlide,
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
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPreviousPdfSlide,
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
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutSetCurrentSlide,
                Devices = new List<Device>() { new Device() { Id = "Testing" } },
                Message = "0"
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetTimeoutPageAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetTimeoutPageRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
