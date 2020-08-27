using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Settings;
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
    public class SettingsControllerTests
    {
        Mock<ILogger<SettingsController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<ISettingsManager> _settingsManager;

        SettingsController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<SettingsController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _settingsManager = new Mock<ISettingsManager>();

            _controller = new SettingsController(_settingsManager.Object, _appLoggerService.Object);
        }

       
    }
}

