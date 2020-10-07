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
    public class DeviceManagerTests
    {
        Mock<IAvidisService> _avidisService;
        Mock<IRecorderService> _recorderService;
        Mock<IMediaService> _mediaService;
        Mock<ISettingsService> _settingsService;
        Mock<IRoutingService> _routingService;
        Mock<ILogger<MediaManager>> _appLoggerService;
        Mock<IMapper> _mapper;
        Mock<IAccessInfoFactory> _accessInfoFactory;

        DevicesManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();
            _settingsService = new Mock<ISettingsService>();
            _appLoggerService = new Mock<ILogger<MediaManager>>();

            _manager = new DevicesManager(_mediaService.Object, _settingsService.Object, _routingService.Object, _appLoggerService.Object, 
                _avidisService.Object, _recorderService.Object, _accessInfoFactory.Object, _mapper.Object);
        }

        #region Pgs

        [Test]
        public void PgsExecutePlayVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo,
                Message = "Sample",
                Devices = new List<Device>() { new Device() { Id = "Preview" } }
            };

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.InitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.InitSessionRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteStopVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsStopVideo,
                Devices = new List<Device>() { new Device() { Id = "Preview" } }
            };

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.DeInitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.DeInitSessionRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteHandleMessageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo,
                Message = "Sample",
                Devices = new List<Device>() { new Device() { Id = "Preview" } }
            };



            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.HandleMessageAsync(It.IsAny<Ism.Streaming.V1.Protos.HandleMessageRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }

        #endregion Pgs

        #region Timeout

        [Test]
        public void TimeoutExecutePlaySlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPlayPdfSlides,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
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

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>()), Times.Once);
            //_mediaService.Verify(mock => mock.TimeoutSetModeAsync(It.Is<Command>(args => args.Message == ((int)TimeoutModes.Pgs).ToString())), Times.Once);

            //Assert.IsNotNull(commandResponse);
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

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetPgsTimeoutModeRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteNextSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutNextPdfSlide,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            
            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.NextPageAsync(), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }


        [Test]
        public void TimeoutExecutePreviousSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPreviousPdfSlide,
                Devices = new List<Device>() { new Device() { Id = "Timeout" } }
            };

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.PreviousPageAsync(), Times.Once);

            //Assert.IsNotNull(commandResponse);
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

            var actionResult = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetTimeoutPageAsync(It.IsAny<Ism.PgsTimeout.Common.Core.SetTimeoutPageRequest>()), Times.Once);

            //Assert.IsNotNull(commandResponse);
        }
        #endregion Timeout
    }
}
