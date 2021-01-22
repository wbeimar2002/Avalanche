using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.ViewModels;
using Avalanche.Api.Tests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Test.Controllers
{
    class ProceduresContollerTests
    {
        Mock<ILogger<ProceduresController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;
        Mock<IProceduresManager> _proceduresManager;

        ProceduresController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<ProceduresController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _proceduresManager = new Mock<IProceduresManager>();

            _controller = new ProceduresController(_appLoggerService.Object, _proceduresManager.Object);

            var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
            mockUrlHelper
                .Setup(
                    x => x.Action(
                        It.IsAny<UrlActionContext>()
                    )
                )
                .Returns("https://example.com")
                .Verifiable();

            _controller.Url = mockUrlHelper.Object;

            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext = context;

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void GetActiveShouldReturnOkResultWithSomeActiveProcedureAsResult()
        {
            var fixture = new Fixture();
            ActiveProcedureViewModel result = fixture.Create<ActiveProcedureViewModel>();

            _proceduresManager.Setup(mock => mock.GetActiveProcedure()).ReturnsAsync(result);

            var okResult = _controller.GetActive(_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {nameof(ProceduresController)}.{nameof(ProceduresController.GetActive)}", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {nameof(ProceduresController)}.{nameof(ProceduresController.GetActive)}", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {nameof(ProceduresController)}.{nameof(ProceduresController.GetActive)}", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }
    }
}
