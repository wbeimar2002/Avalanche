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
    public class PatientsControllerTests
    {
        Mock<ILogger<PatientsController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IPatientsManager> _patientsManager;

        PatientsController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<PatientsController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _patientsManager = new Mock<IPatientsManager>();

            _controller = new PatientsController(_appLoggerService.Object, _patientsManager.Object);
        }
    }
}
