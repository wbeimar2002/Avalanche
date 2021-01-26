using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Managers.PgsTimeout;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
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
    public class DevicesControllerTests
    {
        Mock<ILogger<DevicesController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IDevicesManager> _deviceManager;
        Mock<IPgsTimeoutManager> _pgsTimeoutManager;

        DevicesController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<DevicesController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _deviceManager = new Mock<IDevicesManager>();
            _pgsTimeoutManager = new Mock<IPgsTimeoutManager>();

            _controller = new DevicesController(_deviceManager.Object, _pgsTimeoutManager.Object, _appLoggerService.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void SendCommandReturnOkResult()
        {
            var okResult = _controller.SendCommand(new CommandViewModel(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendCommand", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendCommand", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void SendCommandReturnBadResultIfFails()
        {
            _deviceManager.Setup(mock => mock.SendCommand(It.IsAny<CommandViewModel>(), new Shared.Domain.Models.User())).Throws(It.IsAny<Exception>());

            var badResult = _controller.SendCommand(It.IsAny<CommandViewModel>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.SendCommand", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.SendCommand", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetOperationsOuputsReturnOkResult()
        {
            var okResult = _controller.GetOperationsOuputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetOperationsOuputs", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetOperationsOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetOperationsOuputs", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetOperationsOuputsReturnBadResultIfFails()
        {
            _deviceManager.Setup(mock => mock.GetOperationsOutputs()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetOperationsOuputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetOperationsOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetOperationsOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetOperationsOuputs", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetOperationsSourcesReturnOkResult()
        {
            var okResult = _controller.GetOperationsSources(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetOperationsSources", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetOperationsSources", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetOperationsSources", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetOperationsSourcesReturnBadResultIfFails()
        {
            _deviceManager.Setup(mock => mock.GetOperationsSources()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetOperationsSources(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetOperationsSources", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetOperationsSources", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetOperationsSources", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetPgsOutputsReturnOkResult()
        {
            var okResult = _controller.GetPgsOutputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetPgsOutputs", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetPgsOutputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetPgsOutputs", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetPgsOutputsReturnBadResultIfFails()
        {
            _pgsTimeoutManager.Setup(mock => mock.GetPgsOutputs()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetPgsOutputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetPgsOutputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetPgsOutputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetPgsOutputs", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }

        [Test]
        public void GetTimeoutOuputsReturnOkResult()
        {
            var okResult = _controller.GetTimeoutOuputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetTimeoutOuputs", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetTimeoutOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetTimeoutOuputs", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void GetTimeoutOuputsReturnBadResultIfFails()
        {
            _pgsTimeoutManager.Setup(mock => mock.GetTimeoutOutputs()).Throws(It.IsAny<Exception>());

            var badResult = _controller.GetTimeoutOuputs(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetTimeoutOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetTimeoutOuputs", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetTimeoutOuputs", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
