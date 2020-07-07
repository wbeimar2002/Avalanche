﻿using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.Common.Core;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public partial class MediaServiceTests
    {
        [Test]
        public void TimeoutExecutePlaySlidesShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = ((int)TimeoutModes.Timeout).ToString(),
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var expected = new Empty();

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsTimeoutClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutSetModeAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.SetPgsTimeoutModeAsync(Moq.It.Is<SetPgsTimeoutModeRequest>(arg => arg.Mode == PgsTimeoutModeEnum.PgsTimeoutModeTimeout ), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }


        [Test]
        public void TimeoutExecuteStopSlidesWithAlwaysOnShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = ((int)TimeoutModes.Pgs).ToString(),
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsTimeoutClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutSetModeAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.SetPgsTimeoutModeAsync(Moq.It.Is<SetPgsTimeoutModeRequest>(arg => arg.Mode == PgsTimeoutModeEnum.PgsTimeoutModePgs), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }

        [Test]
        public void TimeoutExecuteStopSlidesWithoutAlwaysOnShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = ((int)TimeoutModes.Idle).ToString(),
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsTimeoutClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutSetModeAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.SetPgsTimeoutModeAsync(Moq.It.Is<SetPgsTimeoutModeRequest>(arg => arg.Mode == PgsTimeoutModeEnum.PgsTimeoutModeIdle), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }

        [Test]
        public void TimeoutExecuteNextSlideShouldReturnResponse()
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
            _mockPgsTimeoutClient.Setup(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutNextSlideAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.NextPageAsync(new Empty()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }

        [Test]
        public void TimeoutExecutePreviousSlideShouldReturnResponse()
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
            _mockPgsTimeoutClient.Setup(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutPreviousSlideAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.PreviousPageAsync(new Empty()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }

        [Test]
        public void TimeoutExecuteSetCurrentSlideShouldReturnResponse()
        {
            Command command = new Command()
            {
                Message = "0",
                SessionId = Guid.NewGuid().ToString(),
                OutputId = "Testing",
                Type = "sample.offer"
            };

            var expected = new Empty();

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockPgsTimeoutClient.Setup(mock => mock.SetTimeoutPageAsync(Moq.It.IsAny<SetTimeoutPageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.IgnoreGrpcServicesMocks = true;
            _service.PgsTimeoutClient = _mockPgsTimeoutClient.Object;

            var actionResult = _service.TimeoutSetCurrentSlideAsync(command);

            _mockPgsTimeoutClient.Verify(mock => mock.SetTimeoutPageAsync(Moq.It.IsAny<SetTimeoutPageRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockPgsTimeoutClient.Object.SetTimeoutPageAsync(new SetTimeoutPageRequest()));
            Assert.AreEqual(actionResult.Result.ResponseCode, 0);
        }
    }
}