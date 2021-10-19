using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Controllers
{
    [TestFixture]
    public class SupportControllerTests
    {
        private Mock<ILogger<SupportController>>? _logger;
        private Mock<IWebHostEnvironment>? _environment;
        private Mock<IMedpresenceManager>? _medpresenceManager;
        private SupportController? _controller;
        private bool _checkLogger;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<SupportController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _medpresenceManager = new Mock<IMedpresenceManager>();

            _controller = new SupportController(_logger.Object, _environment.Object, _medpresenceManager.Object);

            var os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
            {
                _checkLogger = true;
            }
        }

        [Test]
        public async Task GetMedpresenceStateShoulReturnOkResultWithState()
        {
            var fixture = new Fixture();
            var result = fixture.Create<MedpresenceStateViewModel>();

            _medpresenceManager!.Setup(mock => mock.GetMedpresenceStateAsync()).ReturnsAsync(result);

            var okResult = _controller!.GetMedpresenceState();

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetMedpresenceState", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetMedpresenceState", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetMedpresenceState", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(await okResult.ConfigureAwait(false));
        }

        [Test]
        public async Task GetMedpresenceStateShoulReturnBadResultIfFails()
        {

            _medpresenceManager!.Setup(mock => mock.GetMedpresenceStateAsync()).Throws(It.IsAny<Exception>());

            var badResult = await _controller!.GetMedpresenceState().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetMedpresenceState", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetMedpresenceState", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetMedpresenceState", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task StartServiceShouldReturnOkResult()
        {
            var okResult = await _controller!.StartService().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartService", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartService", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StartServiceShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StartServiceAsync()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StartService().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartService", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task StopServiceShouldReturnOkResult()
        {
            var okResult = await _controller!.StopService().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopService", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopService", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StopServiceSessionShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StopServiceAsync()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StopService().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopService", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopService", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task StartServiceRecordingShouldReturnOkResult()
        {
            var okResult = await _controller!.StartServiceRecording().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartServiceRecording", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartServiceRecording", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StartServiceRecordingShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StartRecordingAsyc()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StartServiceRecording().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartServiceRecording", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task StopServiceRecordingShouldReturnOkResult()
        {
            var okResult = await _controller!.StopServiceRecording().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopServiceRecording", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopServiceRecording", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StopServiceRecordingShoulReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StopRecordingAsync()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StopServiceRecording().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopServiceRecording", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopServiceRecording", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task CaptureServiceImageShouldReturnOkResults()
        {
            var okResult = await _controller!.CapptureServiceImage().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.CapptureServiceImage", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.CapptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.CapptureServiceImage", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task CaptureServiceImageShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.CaptureImageAsync()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.CapptureServiceImage().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.CapptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.CapptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.CapptureServiceImage", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}
