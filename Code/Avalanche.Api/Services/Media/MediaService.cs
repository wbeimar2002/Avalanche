using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using System.Threading;
using Avalanche.Shared.Infrastructure.Models;

namespace Avalanche.Api.Services.Media
{
    public class MediaService : IMediaService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        public MediaService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var PgsTimeoutGrpcPort = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            WebRtcStreamerClient = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{WebRTCGrpcPort}");
            PgsTimeoutClient = ClientHelper.GetInsecureClient<PgsTimeout.PgsTimeoutClient>($"https://{_hostIpAddress}:{PgsTimeoutGrpcPort}");
        }

        #region WebRTC
        public async Task<CommandResponse> HandleMessageForVideoAsync(Command command)
        {
            var actionResponse = await WebRtcStreamerClient.HandleMessageAsync(new HandleMessageRequest()
            {
                SessionId = command.SessionId,
                Offer = new WebRtcInfoMessage()
                { 
                    Message = command.Message,
                    Type = command.Type,
                }
            });

            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)actionResponse.ResponseCode
            }; 
        }

        public async Task<CommandResponse> PlayVideoAsync(Command command)
        {
            var applicationName = this.GetType().FullName;

            var actionResponse = await WebRtcStreamerClient.InitSessionAsync(new InitSessionRequest
            {
                AccessInfo = new AccessInfoMessage
                {
                    ApplicationName = applicationName,
                    Details = "Initialize webrtc stream",
                    Id = Guid.NewGuid().ToString(),
                    Ip = _hostIpAddress,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                },
                Quality = RxStreamQualityEnum.RxStreamQualityHdVideo,
                RouteToStreamingEncoder = true,
                StreamId = command.StreamId,
                SessionId = command.SessionId,
                Offer = new WebRtcInfoMessage
                {
                    Aor = "AOR",
                    BypassMaxStreamRestrictions = true,
                    Type = command.Type,
                    Message = command.Message
                }
            });

            var response = new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)actionResponse.ResponseCode,
                Messages = new List<string>()
            };

            foreach (var item in actionResponse.Answer)
            {
                response.Messages.Add(item.Message);
            }

            return response;
        }

        public async Task<CommandResponse> StopVideoAsync(Command command)
        {
            var actionResponse = await WebRtcStreamerClient.DeInitSessionAsync(new DeInitSessionRequest()
            {
                SessionId = command.SessionId,
            });

            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorSuccess
            };
        }

        public async Task<CommandResponse> PlayAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> StopAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> MuteAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> GetVolumeUpAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> GetVolumeDownAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        #endregion WebRTC

        #region PgsTimeout

        public async Task<CommandResponse> PlaySlidesAsync(Command command)
        {
            //Faking calls while I have the real server
            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()
            {
                Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout
            });

            return new CommandResponse()
            {
                OutputId = command.StreamId,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> StopSlidesAsync(Command command)
        {
            //Faking calls while I have the real server
            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.SetPgsTimeoutModeAsync(Moq.It.IsAny<SetPgsTimeoutModeRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.SetPgsTimeoutModeAsync(new SetPgsTimeoutModeRequest()
            {
                Mode = PgsTimeoutModeEnum.PgsTimeoutModeIdle
            });

            return new CommandResponse()
            {
                OutputId = command.StreamId,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> NextSlideAsync(Command command)
        {
            //Faking calls while I have the real server
            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.NextPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.NextPageAsync(new Empty());

            return new CommandResponse()
            {
                OutputId = command.StreamId,
                ResponseCode = 0
            };
        }

        public async Task<CommandResponse> PreviousSlideAsync(Command command)
        {
            //Faking calls while I have the real server
            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.PreviousPageAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.PreviousPageAsync(new Empty());

            return new CommandResponse()
            {
                OutputId = command.StreamId,
                ResponseCode = 0
            };
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            //Faking calls while I have the real server
            Mock<PgsTimeout.PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeout.PgsTimeoutClient>();
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new GetTimeoutPdfPathResponse()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            mockGrpcClient.Setup(mock => mock.GetTimeoutPdfPathAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCall);

            PgsTimeoutClient = mockGrpcClient.Object;

            //Real code starts
            var actionResponse = await PgsTimeoutClient.GetTimeoutPdfPathAsync(new Empty());

            return new TimeoutSettings
            {
                CheckListFileName = actionResponse.PdfPath
            };
        }

        #endregion PgsTimeout
    }
}
