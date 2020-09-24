using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Devices;
using Microsoft.AspNetCore.Hosting;
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

        DevicesController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<DevicesController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _deviceManager = new Mock<IDevicesManager>();

            _controller = new DevicesController(_deviceManager.Object, _appLoggerService.Object);

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }
    }
}
