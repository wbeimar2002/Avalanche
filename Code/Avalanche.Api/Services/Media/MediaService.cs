using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class MediaService : IMediaService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public WebRtcStreamer.WebRtcStreamerClient Client { get; set; }

        public MediaService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            Client = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{WebRTCGrpcPort}");
        }

        public async Task<CommandResponse> HandleMessageForVideoAsync(Command command)
        {
            var actionResponse = await Client.HandleMessageAsync(new HandleMessageRequest()
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

            var actionResponse = await Client.InitSessionAsync(new InitSessionRequest
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
            var actionResponse = await Client.DeInitSessionAsync(new DeInitSessionRequest()
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

        public async Task<CommandResponse> PlaySlidesAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> StopSlidesAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> NextSlideAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }

        public async Task<CommandResponse> PreviousSlideAsync(Command command)
        {
            return new CommandResponse()
            {
                SessionId = command.SessionId,
                OutputId = command.StreamId,
                ResponseCode = (int)WebRtcApiErrorEnum.WebRtcApiErrorUnknown
            };
        }
    }
}
