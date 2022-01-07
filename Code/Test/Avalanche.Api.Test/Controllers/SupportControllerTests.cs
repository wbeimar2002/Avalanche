using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.Medpresence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            var okResult = await _controller!.CaptureServiceImage().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.CaptureServiceImage", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.CaptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.CaptureServiceImage", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task CaptureServiceImageShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.CaptureImageAsync()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.CaptureServiceImage().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.CaptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.CaptureServiceImage", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.CaptureServiceImage", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task DiscardServiceShouldReturnOkResults()
        {
            var okResult = await _controller!.DiscardServiceSession(1234).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DiscardServiceSession", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DiscardServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DiscardServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task DiscardServiceSessionShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.DiscardSessionAsync(It.IsAny<ulong>())).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.DiscardServiceSession(1234).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.DiscardServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.DiscardServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.DiscardServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task ArchiveServiceSessionShouldReturnOkResults()
        {
            var okResult = await _controller!.ArchiveServiceSession(new ArchiveServiceViewModel()).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ArchiveServiceSession", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ArchiveServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ArchiveServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task ArchiveServiceSessionShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.ArchiveSessionAsync(It.IsAny<ArchiveServiceViewModel>())).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.ArchiveServiceSession(new ArchiveServiceViewModel()).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ArchiveServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ArchiveServiceSession", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ArchiveServiceSession", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task GetServiceGuestListShouldReturnOkResult()
        {
            var okResult = await _controller!.GetServiceGuestList().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetServiceGuestList", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetServiceGuestList", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetServiceGuestList", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult);
        }

        [Test]
        public async Task GetServiceGuestListShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.GetGuestList()).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.GetServiceGuestList().ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.GetServiceGuestList", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.GetServiceGuestList", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.GetServiceGuestList", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task InviteGuestShouldReturnOkResult()
        {
            var okResult = await _controller!.InviteGuests(new MedpresenceInviteViewModel{
                Invitees = new List<MedpresenceSecureGuest>()
            }).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.InviteGuests", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.InviteGuests", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.InviteGuests", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task InviteGuestShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.InviteGuests(It.IsAny<MedpresenceInviteViewModel>())).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.InviteGuests(new MedpresenceInviteViewModel
            {
                Invitees = new List<MedpresenceSecureGuest>()
            }).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.InviteGuests", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.InviteGuests", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.InviteGuests", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }

        [Test]
        public async Task ExecuteInSessionCommandShouldReturnOkResult()
        {
            var okResult = await _controller!.ExecuteInSessionCommand(new MedpresenceInSessionCommandViewModel
            {
                Command = "test"
            }).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Never());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task ExecuteInSessionCommandShouldReturnBadResultIfFails()
        {
            _ = _medpresenceManager!.Setup(s => s.ExecuteInSessionCommand(It.IsAny<MedpresenceInSessionCommandViewModel>())).Throws(It.IsAny<Exception>());
            var badResult = await _controller!.ExecuteInSessionCommand(new MedpresenceInSessionCommandViewModel
            {
                Command = "test"
            }).ConfigureAwait(false);

            if (_checkLogger)
            {
                _logger!.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Once());
                _logger!.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.ExecuteInSessionCommand", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);
        }
    }
}
