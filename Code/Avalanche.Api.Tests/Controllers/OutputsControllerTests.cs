using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Ism.Telemetry.RabbitMq.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class OutputsControllerTests
    {
        Mock<ILogger<OutputsController>> _appLoggerService;
        Mock<IOutputsManager> _outputsManager;
        Mock<IMediaManager> _mediaManager;
        Mock<IWebHostEnvironment> _environment;

        OutputsController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<OutputsController>>();
            _outputsManager = new Mock<IOutputsManager>();
            _environment = new Mock<IWebHostEnvironment>();
            _mediaManager = new Mock<IMediaManager>();

            _controller = new OutputsController(_outputsManager.Object, _mediaManager.Object, _appLoggerService.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void GetContentTypeShouldReturnOkResult()
        {
            var contentType = "Sample";

            var okResult = _controller.GetContentType(contentType, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetContentType", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetContentType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetContentType", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetContentTypeShouldReturnBadResultIfFails()
        {
            _outputsManager.Setup(mock => mock.GetContent(It.IsAny<string>())).Throws(It.IsAny<Exception>());
            
            var contentType = "Sample";

            var badResult = _controller.GetContentType(contentType, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetContentType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetContentType", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetContentType", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }


        [Test]
        public void GetAllAvailableShouldReturnOkResult()
        {
            var okResult = _controller.GetAllAvailable(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetAllAvailable", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetAllAvailable", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetAllAvailable", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetAllAvailableShouldReturnBadResultIfFails()
        {
            _outputsManager.Setup(mock => mock.GetAllAvailable()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetAllAvailable(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetAllAvailable", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetAllAvailable", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetAllAvailable", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }


        [Test]
        public void GetCurrentStateShouldReturnOkResult()
        {
            string outputId = "Sample";
            var okResult = _controller.GetCurrentState(outputId, It.IsAny<StateTypes>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetCurrentState", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetCurrentState", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetCurrentState", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetCurrentStateShouldReturnBadResultIfFails()
        {
            string outputId = "Sample";
            _outputsManager.Setup(mock => mock.GetCurrentState(It.IsAny<string>(), It.IsAny<StateTypes>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetCurrentState(outputId, It.IsAny<StateTypes>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetCurrentState", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetCurrentState", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetCurrentState", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetCurrentStatesShouldReturnOkResult()
        {
            string outputId = "Sample";
            var okResult = _controller.GetCurrentStates(outputId, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetCurrentStates", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetCurrentStates", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetCurrentStates", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetCurrentStatesShouldReturnBadResultIfFails()
        {
            string outputId = "Sample";
            _outputsManager.Setup(mock => mock.GetCurrentStates(It.IsAny<string>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetCurrentStates(outputId, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetCurrentStates", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetCurrentStates", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetCurrentStates", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void SendCommandShouldReturnOkResult()
        {
            var commandViewModel = new CommandViewModel()
            {
                Outputs = new List<Output>() { new Output() { Id = "Sample" } }
            };

            var okResult = _controller.SendCommand(commandViewModel, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendCommand", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendCommand", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SendCommandShouldReturnBadResultIfNoOutputs()
        {
            var commandViewModel = new CommandViewModel()
            {
                Outputs = new List<Output>() { new Output() { Id = "Sample" } }
            };

            _mediaManager.Setup(mock => mock.SendCommandAsync(It.IsAny<CommandViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.SendCommand(commandViewModel, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendCommand", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void SendCommandShouldReturnBadResultIfFails()
        {
            var commandViewModel = new CommandViewModel()
            {
                Outputs = new List<Output>() { new Output() { Id = "Sample" } }
            };

            _mediaManager.Setup(mock => mock.SendCommandAsync(It.IsAny<CommandViewModel>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.SendCommand(commandViewModel, _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendCommand", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetTimeoutSettingsShouldReturnOkResult()
        {
            _mediaManager.Setup(mock => mock.GetTimeoutSettingsAsync());

            var okResult = _controller.GetTimeoutSettings(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetTimeoutSettings", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetTimeoutSettings", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetTimeoutSettings", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetTimeoutSettingsShouldReturnBadResultIfFails()
        {
            _mediaManager.Setup(mock => mock.GetTimeoutSettingsAsync()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetTimeoutSettings(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetTimeoutSettings", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetTimeoutSettings", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetTimeoutSettings", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
