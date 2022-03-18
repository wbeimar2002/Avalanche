using System;
using AutoFixture;
using Avalanche.Api.Controllers.V1;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Tests.Extensions;
using Avalanche.Api.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Avalanche.Api.Test.Controllers
{
    class ProceduresControllerTests
    {
        Mock<ILogger<ProceduresController>> _logger;
        Mock<IWebHostEnvironment> _environment;
        Mock<IProceduresManager> _proceduresManager;

        ProceduresController _controller;
        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ProceduresController>>();
            _environment = new Mock<IWebHostEnvironment>();
            _proceduresManager = new Mock<IProceduresManager>();

            _controller = new ProceduresController(_logger.Object, _proceduresManager.Object, _environment.Object);

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
        public void BasicSearchShouldReturnOkResultWithSomeItemsAsResult()
        {
            var fixture = new Fixture();
            var result = fixture.Create<ProceduresContainerViewModel>();
            var filter = new ProcedureSearchFilterViewModel()
            {
                Page = 0,
                Limit = 25
            };

            _proceduresManager.Setup(mock => mock.Search(filter)).ReturnsAsync(result);

            var okResult = _controller.Search(filter);

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {nameof(ProceduresController)}.{nameof(ProceduresController.Search)}", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {nameof(ProceduresController)}.{nameof(ProceduresController.Search)}", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {nameof(ProceduresController)}.{nameof(ProceduresController.Search)}", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void CreateDownloadRequestControllerTest()
        {
            var request = new ProcedureDownloadRequestViewModel
            {
                ProcedureId = new ProcedureIdViewModel("2021_11_08T21_51_25_TODO", "cache"),
                ContentItemIds = new System.Collections.Generic.List<string>
                {
                    "3206c25e-d9eb-47e0-80c8-96f5233be969",
                    "8378ff3d-37ac-49fa-b3aa-4c6bda01b41d"
                },
                RequestId = "1642114410758"
            };

            _proceduresManager.Setup(mock => mock.DownloadProcedure(request));

            var result = _controller.CreateDownloadRequest(
                request.ProcedureId.Id,
                request.ProcedureId.RepositoryId,
                request.RequestId,
                request.ContentItemIds
            );

            if (_checkLogger)
            {
                _logger.Verify(LogLevel.Error, $"Exception {nameof(ProceduresController)}.{nameof(ProceduresController.CreateDownloadRequest)}", Times.Never());
                _logger.Verify(LogLevel.Debug, $"Requested {nameof(ProceduresController)}.{nameof(ProceduresController.CreateDownloadRequest)}", Times.Once());
                _logger.Verify(LogLevel.Debug, $"Completed {nameof(ProceduresController)}.{nameof(ProceduresController.CreateDownloadRequest)}", Times.Once());
            }

            Assert.IsInstanceOf<OkResult>(result.Result);
        }
    }
}
