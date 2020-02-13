using Avalanche.Api.Controllers.V1;
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
    public class UsersControllerTests
    {
        Mock<ILogger<UsersController>> _appLoggerService;
        Mock<IWebHostEnvironment> _environment;

        UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _appLoggerService = new Mock<ILogger<UsersController>>();
            _environment = new Mock<IWebHostEnvironment>();

            _controller = new UsersController(_appLoggerService.Object);
        }
    }
}