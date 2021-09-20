using AutoMapper;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class DeviceControllerTests
    {
        Mock<ILogger<DeviceController>> _logger;
        Mock<IWebHostEnvironment> _environment;
        Mock<IRoutingManager> _routingManager;
        Mock<IMapper> _autoMapper;

        DeviceController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<DeviceController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _routingManager = new Mock<IRoutingManager>();
            _routingManager = new Mock<IRoutingManager>();
            _autoMapper = new Mock<IMapper>();
            _controller = new DeviceController(_logger.Object, _routingManager.Object, _autoMapper.Object, _environment.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public async Task DevicesController_ShowPreview_Verify_Success()
        {
            var vm = new RoutingPreviewViewModel()
            {
                Index = 0,
                Region = new Shared.Domain.Models.Media.RegionModel
                {
                    X = 100,
                    Y = 100,
                    Width = 100,
                    Height = 100
                }
            };

            var okResult = await _controller.ShowPreview(vm);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {nameof(DeviceController)}.{nameof(DeviceController.ShowPreview)}", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {nameof(DeviceController)}.{nameof(DeviceController.ShowPreview)}", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {nameof(DeviceController)}.{nameof(DeviceController.ShowPreview)}", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }

        [Test]
        public async Task DevicesController_HidePreview_Verify_Success()
        {
            var vm = new RoutingPreviewViewModel()
            {
                Index = 0,
                Region = new Shared.Domain.Models.Media.RegionModel
                {
                    X = 100,
                    Y = 100,
                    Width = 100,
                    Height = 100
                }
            };

            var okResult = await _controller.HidePreview(vm);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {nameof(DeviceController)}.{nameof(DeviceController.HidePreview)}", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {nameof(DeviceController)}.{nameof(DeviceController.HidePreview)}", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {nameof(DeviceController)}.{nameof(DeviceController.HidePreview)}", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(okResult);
        }
    }
}
