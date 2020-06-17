using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Castle.Core.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class MediaManagerTests
    {
        Mock<IMediaService> _mediaService;
        Mock<ISettingsService> _settingsService;
        Mock<ILogger<MediaManager>> _appLoggerService;

        MediaManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();
            _settingsService = new Mock<ISettingsService>();
            _appLoggerService = new Mock<ILogger<MediaManager>>();

            _manager = new MediaManager(_mediaService.Object, _settingsService.Object, _appLoggerService.Object);
        }

        #region Pgs

        [Test]
        public void PgsExecutePlayVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo,
                Message = "Sample",
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsPlayVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsPlayVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteStopVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsStopVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsStopVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsStopVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteHandleMessageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo,
                Message = "Sample",
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsHandleMessageForVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsHandleMessageForVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecutePlayAudioShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsPlayAudio,
                Outputs = new List<Output>() { new Output() { Id = "Testing" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsPlayAudioAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsPlayAudioAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteStopAudioShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsStopAudio,
                Outputs = new List<Output>() { new Output() { Id = "Testing" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsStopAudioAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsStopAudioAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
        #endregion Pgs

        #region Timeout

        [Test]
        public void TimeoutExecutePlaySlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPlayPdfSlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModePgsIfAlwaysOnIsTrue()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            TimeoutSettings timeoutSettings = new TimeoutSettings()
            {
                PgsVideoAlwaysOn = true
            };

            _settingsService.Setup(mock => mock.GetTimeoutSettingsAsync()).ReturnsAsync(timeoutSettings);
            _mediaService.Setup(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutSetModeAsync(It.Is<Command>(args => args.Message == ((int)TimeoutModes.Pgs).ToString())), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteStopSlidesShouldSetTimeoutModeIdleIfAlwaysOnIsFalse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            TimeoutSettings timeoutSettings = new TimeoutSettings()
            {
                PgsVideoAlwaysOn = false
            };

            _settingsService.Setup(mock => mock.GetTimeoutSettingsAsync()).ReturnsAsync(timeoutSettings);
            _mediaService.Setup(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutSetModeAsync(It.Is<Command>(args => args.Message == ((int)TimeoutModes.Idle).ToString())), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void TimeoutExecuteNextSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutNextPdfSlide,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.TimeoutNextSlideAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutNextSlideAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }


        [Test]
        public void TimeoutExecutePreviousSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutPreviousPdfSlide,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.TimeoutPreviousSlideAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutPreviousSlideAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }


        [Test]
        public void TimeoutExecuteSetCurrentPageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutSetCurrentSlide,
                Outputs = new List<Output>() { new Output() { Id = "Testing" } },
                Message = "0"
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.TimeoutSetCurrentSlideAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutSetCurrentSlideAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
        #endregion Timeout
    }
}
