using Avalanche.Api.Services.Media;
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

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public class MediaServiceTests
    {
        Mock<IConfigurationService> _configurationService;
        Moq.Mock<WebRtcStreamer.WebRtcStreamerClient> _mockGrpcClient;
        MediaService _service;

        [SetUp]
        public void Setup()
        {
            _configurationService = new Mock<IConfigurationService>();
            _mockGrpcClient = new Moq.Mock<WebRtcStreamer.WebRtcStreamerClient>();

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificateFile = assemblyFolder + @"/grpc_localhost_root_l1.pfx";

            if (!File.Exists(certificateFile))
                Console.WriteLine("Test certificate can not be reached.");

            _configurationService.Setup(mock => mock.GetEnvironmentVariable("hostIpAddress")).Returns("10.0.75.1");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("WebRTCGrpcPort")).Returns("8001");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcCertificate")).Returns(certificateFile);
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcPassword")).Returns("0123456789");

            _service = new MediaService(_configurationService.Object);
        }

        [Test]
        public void ExecutePlayShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                StreamId = "Testing",
                Type = "sample.offer"
            };

            var expected = new InitSessionResponse()
            {
                ResponseCode = WebRtcApiErrorEnum.WebRtcApiErrorSuccess,
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new InitSessionResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockGrpcClient.Setup(mock => mock.InitSessionAsync(Moq.It.IsAny<InitSessionRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockGrpcClient.Object;

            var actionResult = _service.PlayVideoAsync(command);

            _mockGrpcClient.Verify(mock => mock.InitSessionAsync(Moq.It.IsAny<InitSessionRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockGrpcClient.Object.InitSessionAsync(new InitSessionRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, (int)expected.ResponseCode);
        }

        [Test]
        public void ExecuteStopShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                StreamId = "Testing",
                Type = "sample.offer"
            };

            var expected = new Empty();

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockGrpcClient.Setup(mock => mock.DeInitSessionAsync(Moq.It.IsAny<DeInitSessionRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockGrpcClient.Object;

            var actionResult = _service.StopVideoAsync(command);

            _mockGrpcClient.Verify(mock => mock.DeInitSessionAsync(Moq.It.IsAny<DeInitSessionRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockGrpcClient.Object.DeInitSessionAsync(new DeInitSessionRequest()));
            Assert.AreEqual(0, (int)actionResult.Result.ResponseCode);
        }

        [Test]
        public void ExecuteHandleMessageShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "Sample",
                SessionId = Guid.NewGuid().ToString(),
                StreamId = "Testing",
                Type = "sample.offer"
            };

            var expected = new HandleMessageResponse()
            {
                ResponseCode = WebRtcApiErrorEnum.WebRtcApiErrorSuccess,
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new HandleMessageResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockGrpcClient.Setup(mock => mock.HandleMessageAsync(Moq.It.IsAny<HandleMessageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.WebRtcStreamerClient = _mockGrpcClient.Object;

            var actionResult = _service.HandleMessageForVideoAsync(command);

            _mockGrpcClient.Verify(mock => mock.HandleMessageAsync(Moq.It.IsAny<HandleMessageRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockGrpcClient.Object.HandleMessageAsync(new HandleMessageRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, (int)expected.ResponseCode);
        }
    }
}
