using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.V1.Protos;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        public bool UseMocks { get; set; }
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        #region PgsTimeout

        public async Task SetPgsTimeoutModeAsync(SetPgsTimeoutModeRequest setPgsTimeoutModeRequest)
        {
            //Faking calls while I have the real server
            if (UseMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            await PgsTimeoutClient.SetPgsTimeoutModeAsync(setPgsTimeoutModeRequest);
        }

        public async Task SetTimeoutPageAsync(SetTimeoutPageRequest setTimeoutPageRequest)
        {
            //Faking calls while I have the real server
            if (UseMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SetTimeoutPageAsync(Moq.It.IsAny<SetTimeoutPageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            await PgsTimeoutClient.SetTimeoutPageAsync(setTimeoutPageRequest);
        }

        public async Task NextPageAsync()
        {
            //Faking calls while I have the real server
            if (UseMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            await PgsTimeoutClient.NextPageAsync(new Empty());
        }

        public async Task PreviousPageAsync()
        {
            //Faking calls while I have the real server
            if (UseMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            await PgsTimeoutClient.PreviousPageAsync(new Empty());
        }

        #endregion PgsTimeout
    }
}
