using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public class MediaServiceTests
    {
        MediaService _service;

        [SetUp]
        public void Setup()
        {
            _service = new MediaService();
        }

        [Test]
        public void ExecutePlayShouldReturnResponse()
        {
            Command command = new Command();

            var actionResult = _service.PlayAsync(command);

            Assert.IsNotNull(actionResult.Result);
        }
    }
}
