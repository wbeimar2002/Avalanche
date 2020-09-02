using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.PgsTimeout.Common.Core;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        readonly IConfigurationService _configurationService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }

        public MediaService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            WebRtcStreamerClient = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
            PgsTimeoutClient = ClientHelper.GetInsecureClient<PgsTimeout.PgsTimeoutClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        #region WebRTC

        public async Task<CommandResponse> PgsHandleMessageForVideoAsync(Command command)
        {
            var actionResponse = await WebRtcStreamerClient.HandleMessageAsync(new HandleMessageRequest()
            {
                SessionId = command.AdditionalInfo,
                Offer = new WebRtcInfoMessage()
                { 
                    Message = command.Message,
                    Type = command.Type,
                }
            });

            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)actionResponse.ResponseCode
            }; 
        }

        public async Task<CommandResponse> PgsPlayVideoAsync(Command command)
        {
            var applicationName = this.GetType().FullName;
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var actionResponse = await WebRtcStreamerClient.InitSessionAsync(new InitSessionRequest
            {
                AccessInfo = new AccessInfoMessage
                {
                    ApplicationName = applicationName,
                    Details = "Initialize webrtc stream",
                    Id = Guid.NewGuid().ToString(),
                    Ip = accessInfo.Ip,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                },
                Quality = RxStreamQualityEnum.RxStreamQualityHdVideo,
                RouteToStreamingEncoder = true,
                StreamId = command.Source.Id,
                SessionId = command.AdditionalInfo,
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
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)actionResponse.ResponseCode,
                Messages = new List<string>()
            };

            foreach (var item in actionResponse.Answer)
            {
                response.Messages.Add(item.Message);
            }

            return response;
        }

        public async Task<CommandResponse> PgsStopVideoAsync(Command command)
        {
            var actionResponse = await WebRtcStreamerClient.DeInitSessionAsync(new DeInitSessionRequest()
            {
                SessionId = command.AdditionalInfo,
            });

            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorSuccess
            };
        }

        public async Task<CommandResponse> PgsPlayAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> PgsStopAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> PgsMuteAudioAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> PgsGetAudioVolumeUpAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> PgsGetAudioVolumeDownAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.AdditionalInfo,
                Device = command.Source,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        #endregion WebRTC
    }
}
