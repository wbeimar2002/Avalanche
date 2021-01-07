using Avalanche.Api.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Avalanche.Api.Tests.Utility
{
    [TestFixture()]
    public class AccessInfoFactoryTests
    {
        [Test]
        public void ExecuteGenerateAccessInfoSucceeds()
        {
            Mock<IHttpContextAccessor> mock = new Mock<IHttpContextAccessor>();

            string ip = "192.168.0.1";
            string user = "user";
            string details = "details";

            var mockContext = CreateValidMockHttpContext(ip, user);
            mock.Setup(m => m.HttpContext).Returns(mockContext.Object);

            AccessInfoFactory accessInfoFactory = new AccessInfoFactory(mock.Object);

            var accessInfo = accessInfoFactory.GenerateAccessInfo(details);

            Assert.NotNull(accessInfo);
            Assert.AreEqual(ip, accessInfo.Ip);
            Assert.AreEqual(user, accessInfo.UserName);
            Assert.AreEqual(details, accessInfo.Details);
            Assert.AreEqual(Environment.MachineName, accessInfo.MachineName);
            Assert.AreEqual("AvalancheApi", accessInfo.ApplicationName);
        }

        private Mock<HttpContext> CreateValidMockHttpContext(string testIp, string username)
        {
            Mock<HttpContext> mock = new Mock<HttpContext>();

            mock.Setup(c => c.Request.Headers).Returns(new HeaderDictionary());
            mock.Setup(c => c.Connection.RemoteIpAddress).Returns(IPAddress.Parse(testIp));

            mock.Setup(c => c.User.Identity.Name).Returns(username);
            return mock;
        }
    }
}
