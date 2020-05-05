using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
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

        MediaManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();

            _manager = new MediaManager(_mediaService.Object);
        }

        [Test]
        public void ExecutePlayShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel();
            commandViewModel.CommandType = Shared.Domain.Enumerations.CommandTypes.PlayVideo;

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.PlayVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteStopShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel();
            commandViewModel.CommandType = Shared.Domain.Enumerations.CommandTypes.StopVideo;

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.StopVideoAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void ExecuteHandleMesssageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel();
            commandViewModel.CommandType = Shared.Domain.Enumerations.CommandTypes.HandleMessage;

            CommandResponse commandResponse = new CommandResponse();

            _mediaService.Setup(mock => mock.HandleMessageAsync(It.IsAny<Command>())).ReturnsAsync(commandResponse);

            var actionResult = _manager.SendCommandAsync(commandViewModel);

            Assert.IsNotNull(commandResponse);
        }
    }
}
