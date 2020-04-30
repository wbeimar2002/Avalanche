using Avalanche.Shared.Domain.Models;
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
        private static void GetClient(out string hostIpAddress, out WebRtcStreamer.WebRtcStreamerClient client)
        {
            hostIpAddress = Environment.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = Environment.GetEnvironmentVariable("WebRTCGrpcPort");
            var grpcCertificate = Environment.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = Environment.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
        }

        public async Task<CommandResponse> HandleMesssage(Command command)
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

        public async Task<CommandResponse> Play(Command command)
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

        public async Task<CommandResponse> Stop(Command command)
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
