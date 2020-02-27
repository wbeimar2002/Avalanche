using Avalanche.Api.Controllers.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using Avalanche.Api.Tests.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Security;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.Tests.Controllers
{
    [TestFixture()]
    public class AuthorizationControllerTests
    {
        Mock<ISecurityManager> _securityManager;
        Mock<ILogger<AuthorizationController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;

        AuthorizationController _controller;

        bool _checkLogger = false;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<AuthorizationController>>();
            _securityManager = new Mock<ISecurityManager>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new AuthorizationController(_securityManager.Object, _appLoggerService.Object) ;

            OperatingSystem os = Environment.OSVersion;

            if (os.Platform == PlatformID.Win32NT)
                _checkLogger = true;
        }

        [Test]
        public void AuthenticateShouldReturnOkResult()
        {
            string token = @"{
                  ""access_token"": ""eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRG9ja2VySG9zdCIsInN1YiI6IkRvY2tlckhvc3QiLCJqdGkiOiIxNDI5MGUyNi0xYjdiLTQ1NTAtYmI3OC1lYWI5NzMwZWEwMmQiLCJpYXQiOjE1ODAzNzczMTEsIlVzZXJUeXBlIjoiQXZhbGFuY2hlRGV2TW9kZSIsIm5iZiI6MTU4MDM3NzMxMCwiZXhwIjoxNTgwNDEzMzEwfQ."",
                  ""expires_in"": 36000
                }";

            _securityManager.Setup(mock => mock.Authenticate(It.IsAny<User>())).ReturnsAsync(token);

            var user = new User();
            var okResult = _controller.Authenticate(user,_environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Authenticate", Times.Never());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Authenticate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Authenticate", Times.Once());
            }

            Assert.IsInstanceOf<OkObjectResult>(okResult.Result);
        }

        [Test]
        public void AuthenticateShouldReturnBadResultIfFails()
        {
            _securityManager.Setup(mock => mock.Authenticate(It.IsAny<User>())).Throws(It.IsAny<Exception>());

            var badResult = _controller.Authenticate(It.IsAny<User>(), _environment.Object);

            if (_checkLogger)
            {
                _appLoggerService.Verify(LogLevel.Error, $"Exception {_controller.GetType().Name}.Authenticate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Requested {_controller.GetType().Name}.Authenticate", Times.Once());
                _appLoggerService.Verify(LogLevel.Debug, $"Completed {_controller.GetType().Name}.Authenticate", Times.Once());
            }

            Assert.IsInstanceOf<BadRequestObjectResult>(badResult.Result);
        }
    }
}
