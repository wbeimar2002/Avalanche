using Avalanche.Api.Utility;
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
    public class HttpContextUtilityTests
    {
        [Test]
        public void ExecuteHttpContextGetRequestIpConnectionRemoteAddressSucceeds()
        {
            Mock<HttpContext> mock = new Mock<HttpContext>();

            string testIp = "192.168.0.1";

            mock.Setup(c => c.Request.Headers).Returns(new HeaderDictionary());
            mock.Setup(c => c.Connection.RemoteIpAddress).Returns(IPAddress.Parse(testIp));

            string result = HttpContextUtilities.GetRequestIP(mock.Object);
            Assert.AreEqual(testIp, result);
        }


        [Test]
        public void ExecuteHttpContextGetRequestIpForwardedAddressHeaderSucceeds()
        {
            Mock<HttpContext> mock = new Mock<HttpContext>();

            string testIp = "192.168.0.1";

            mock.Setup(c => c.Request.Headers).Returns(GetForwardedHeaders(testIp));
            mock.Setup(c => c.Connection.RemoteIpAddress).Returns(null as IPAddress);

            string result = HttpContextUtilities.GetRequestIP(mock.Object);
            Assert.AreEqual(testIp, result);
        }

        [Test]
        public void ExecuteHttpContextGetRequestIpRemoteAddrHeaderSucceeds()
        {
            Mock<HttpContext> mock = new Mock<HttpContext>();

            string testIp = "192.168.0.1";

            mock.Setup(c => c.Request.Headers).Returns(GetRemoteAddrHeaders(testIp));
            mock.Setup(c => c.Connection.RemoteIpAddress).Returns(null as IPAddress);

            string result = HttpContextUtilities.GetRequestIP(mock.Object);
            Assert.AreEqual(testIp, result);
        }


        private HeaderDictionary GetForwardedHeaders(string ip)
        {
            HeaderDictionary headers = new HeaderDictionary();
            headers.Add("X-Forwarded-For", ip);
            return headers;
        }
        private HeaderDictionary GetRemoteAddrHeaders(string ip)
        {
            HeaderDictionary headers = new HeaderDictionary();
            headers.Add("REMOTE_ADDR", ip);
            return headers;
        }
    }
}
