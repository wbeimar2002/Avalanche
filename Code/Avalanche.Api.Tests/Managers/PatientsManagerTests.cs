using Avalanche.Api.Managers.Health;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class PatientsManagerTests
    {
        Mock<IPieService> _pieService;
        Mock<IAccessInfoFactory> _accessInfoFactory;

        PatientsManager _manager;

        [SetUp]
        public void Setup()
        {
            _pieService = new Mock<IPieService>();
            _manager = new PatientsManager(_pieService.Object);
        }
    }
}
