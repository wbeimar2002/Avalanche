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

        public MediaService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        private void GetClient(out string hostIpAddress, out WebRtcStreamer.WebRtcStreamerClient client)
        {
            hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            client = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}");
        }

        public async Task<CommandResponse> HandleMessageAsync(Command command)
        {
            string hostIpAddress;
            WebRtcStreamer.WebRtcStreamerClient client;

            GetClient(out hostIpAddress, out client);

            var actionResponse = await client.HandleMessageAsync(new HandleMessageRequest()
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

        public async Task<CommandResponse> PlayAsync(Command command)
        {
            string hostIpAddress;
            WebRtcStreamer.WebRtcStreamerClient client;

            GetClient(out hostIpAddress, out client);

            var applicationName = this.GetType().FullName;

            var actionResponse = await client.InitSessionAsync(new InitSessionRequest
            {
                AccessInfo = new AccessInfoMessage
                {
                    ApplicationName = applicationName,
                    Details = "Initialize webrtc stream",
                    Id = Guid.NewGuid().ToString(),
                    Ip = hostIpAddress,
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
            }); ;

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

        public async Task<CommandResponse> StopAsync(Command command)
        {
            string hostIpAddress;
            WebRtcStreamer.WebRtcStreamerClient client;

            GetClient(out hostIpAddress, out client);

            var actionResponse = await client.DeInitSessionAsync(new DeInitSessionRequest()
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
    }
}
