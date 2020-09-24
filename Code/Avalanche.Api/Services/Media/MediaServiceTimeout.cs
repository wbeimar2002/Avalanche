using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        #region PgsTimeout

        public async Task<CommandResponse> TimeoutSetModeAsync(Command command)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            var actionResponse = await PgsTimeoutClient.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()
            {
                Mode = (PgsTimeoutModeEnum)Convert.ToInt32(command.Message)
            });

            return new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> TimeoutSetCurrentSlideAsync(Command command)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SetTimeoutPageAsync(Moq.It.IsAny<SetTimeoutPageRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            var actionResponse = await PgsTimeoutClient.SetTimeoutPageAsync(new SetTimeoutPageRequest()
            {
                PageNumber = Convert.ToInt32(command.Message)
            });

            return new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> TimeoutNextSlideAsync(Command command)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            var actionResponse = await PgsTimeoutClient.NextPageAsync(new Empty());

            return new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> TimeoutPreviousSlideAsync(Command command)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PgsTimeoutClient = mockGrpcClient.Object;
            }

            var actionResponse = await PgsTimeoutClient.PreviousPageAsync(new Empty());

            return new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = 0
            };
        }

        #endregion PgsTimeout
    }
}
