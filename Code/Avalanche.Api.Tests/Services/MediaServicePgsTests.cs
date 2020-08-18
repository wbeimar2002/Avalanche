﻿using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Streaming.Common.Core;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Testing;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using System.IO;
using System.Reflection;
using Ism.PgsTimeout.Common.Core;
using Avalanche.Api.Utilities;
using Ism.IsmLogCommon.Core;

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public partial class MediaServiceTests
    {
        Mock<IConfigurationService> _configurationService;
        Mock<IAccessInfoFactory> _accessInfoFactory;

        Moq.Mock<WebRtcStreamer.WebRtcStreamerClient> _mockPgsGrpcClient;
        Moq.Mock<PgsTimeout.PgsTimeoutClient> _mockPgsTimeoutClient;

        MediaService _service;

        [SetUp]
        public void Setup()
        {
            _configurationService = new Mock<IConfigurationService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _mockPgsGrpcClient = new Moq.Mock<WebRtcStreamer.WebRtcStreamerClient>();
            _mockPgsTimeoutClient = new Moq.Mock<PgsTimeout.PgsTimeoutClient>();

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificateFile = assemblyFolder + @"/grpc_localhost_root_l1.pfx";

            if (!File.Exists(certificateFile))
                Console.WriteLine("Test certificate can not be reached.");

            _configurationService.Setup(mock => mock.GetEnvironmentVariable("hostIpAddress")).Returns("10.0.75.1");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("WebRTCGrpcPort")).Returns("8001");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcCertificate")).Returns(certificateFile);
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcPassword")).Returns("0123456789");

            _accessInfoFactory.Setup(mock => mock.GenerateAccessInfo(It.IsAny<string>())).Returns(new Ism.IsmLogCommon.Core.AccessInfo("192.168.0.1", "use", "app", "machine", "details", false));

            _service = new MediaService(_configurationService.Object, _accessInfoFactory.Object);
        }

        [Test]
        public void PgsExecutePlayVideoShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var expected = new InitSessionResponse()
            {
                ResponseCode = WebRtcApiErrorEnum.WebRtcApiErrorSuccess,
            };

            var response = new InitSessionResponse()
            {
                ResponseCode = WebRtcApiErrorEnum.WebRtcApiErrorSuccess,
            };

            response.Answer.Add(new WebRtcInfoMessage() { Message = "Sample" });

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(response), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsGrpcClient.Setup(mock => mock.InitSessionAsync(Moq.It.IsAny<InitSessionRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockPgsGrpcClient.Object;

            var actionResult = _service.PgsPlayVideoAsync(command);

            _mockPgsGrpcClient.Verify(mock => mock.InitSessionAsync(Moq.It.IsAny<InitSessionRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsGrpcClient.Object.InitSessionAsync(new InitSessionRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, (int)expected.ResponseCode);
        }

        [Test]
        public void PgsExecuteStopVideoShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var expected = new Empty();

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsGrpcClient.Setup(mock => mock.DeInitSessionAsync(Moq.It.IsAny<DeInitSessionRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockPgsGrpcClient.Object;

            var actionResult = _service.PgsStopVideoAsync(command);

            _mockPgsGrpcClient.Verify(mock => mock.DeInitSessionAsync(Moq.It.IsAny<DeInitSessionRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsGrpcClient.Object.DeInitSessionAsync(new DeInitSessionRequest()));
            Assert.AreEqual(0, (int)actionResult.Result.ResponseCode);
        }

        [Test]
        public void PgsExecuteHandleMessageForViewoShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var expected = new HandleMessageResponse()
            {
                ResponseCode = WebRtcApiErrorEnum.WebRtcApiErrorSuccess,
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new HandleMessageResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsGrpcClient.Setup(mock => mock.HandleMessageAsync(Moq.It.IsAny<HandleMessageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockPgsGrpcClient.Object;

            var actionResult = _service.PgsHandleMessageForVideoAsync(command);

            _mockPgsGrpcClient.Verify(mock => mock.HandleMessageAsync(Moq.It.IsAny<HandleMessageRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsGrpcClient.Object.HandleMessageAsync(new HandleMessageRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, (int)expected.ResponseCode);
        }
    }
}
