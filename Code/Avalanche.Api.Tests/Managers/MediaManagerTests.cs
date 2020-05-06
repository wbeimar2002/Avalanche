using Avalanche.Api.Managers.Devices;
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
        Mock<IConfigurationService> _configurationService;

        MediaManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();
            _configurationService = new Mock<IConfigurationService>();

            _manager = new MediaManager(_mediaService.Object);
        }

        [Test]
        public void ExecutePlayShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PlayVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PlayVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PlayVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteStopShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.StopVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.StopVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.StopVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteHandleMessageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.HandleMessageForVideo,
                Outputs = new List<Output>() { new Output() { Id = "Preview" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.HandleMessageForVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.HandleMessageForVideoAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecutePlaySlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PlaySlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PlaySlidesAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PlaySlidesAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteStopSlidesShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.StopSlides,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.StopSlidesAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.StopSlidesAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteNextSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.NextSlide,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.NextSlideAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.NextSlideAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }


        [Test]
        public void ExecutePreviousSlideShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PreviousSlide,
                Outputs = new List<Output>() { new Output() { Id = "Timeout" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PreviousSlideAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PreviousSlideAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }


        [Test]
        public void ExecutePlayAudioShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.PlayAudio,
                Outputs = new List<Output>() { new Output() { Id = "Testing" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PlayAudioAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.PlayAudioAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteStopAudioShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = Shared.Domain.Enumerations.CommandTypes.StopAudio,
                Outputs = new List<Output>() { new Output() { Id = "Testing" } }
            };

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.StopAudioAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            _mediaService.Verify(mock => mock.StopAudioAsync(It.IsAny<Command>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
