using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Licencing;
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
    public class ProceduresControllerTests
    {
        Mock<ILogger<ProceduresController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<ILicensingManager> _licensingManager;

        ProceduresController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<ProceduresController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _licensingManager = new Mock<ILicensingManager>();

            _controller = new ProceduresController(_appLoggerService.Object);
        }
    }
}
