using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Health;
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
    public class PhysiciansControllerTests
    {
        Mock<ILogger<PhysiciansController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPhysiciansManager> _physiciansManager;

        PhysiciansController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<PhysiciansController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _physiciansManager = new Mock<IPhysiciansManager>();

            _controller = new PhysiciansController(_appLoggerService.Object, _physiciansManager.Object);
        }
    }
}
