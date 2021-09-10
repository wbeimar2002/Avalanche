using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Tests.Extensions;
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
        public async Task StartServiceSessionShouldReturnOkResult()
        {
            var okResult = await _controller!.StartServiceSession().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartServiceSession", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StartServiceSessionShouldReturBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StartServiceSession()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StartServiceSession().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StartServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StartServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StartServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task StopServiceSessionShouldReturnOkResult()
        {
            var okResult = await _controller!.StopServiceSession().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopServiceSession", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task StopServiceSessionShouldReturBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.StopServiceSession()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.StopServiceSession().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.StopServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.StopServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.StopServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}
