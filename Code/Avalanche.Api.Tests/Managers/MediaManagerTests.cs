using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Castle.Core.Configuration;
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

        MediaManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();
            _settingsService = new Mock<ISettingsService>();

            _manager = new MediaManager(_mediaService.Object, _settingsService.Object);
        }

        [Test]
        public void ExecutePlayShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsPlayVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsPlayVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteStopShouldReturnResponse()
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
        public void ExecuteHandleMessageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PgsHandleMessageForVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PgsHandleMessageForVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecutePlaySlidesShouldReturnResponse()
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
        public void ExecuteStopSlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.TimeoutSetModeAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteNextSlideShouldReturnResponse()
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
        public void ExecutePreviousSlideShouldReturnResponse()
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
        public void ExecutePlayAudioShouldReturnResponse()
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
        public void ExecuteStopAudioShouldReturnResponse()
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
    }
}
