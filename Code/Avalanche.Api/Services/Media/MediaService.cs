using Avalanche.Shared.Domain.Models;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class MediaService : IMediaService
    {
        public async Task<CommandResponse> Play(string sessionId, string streamId, string message)
        {
            var endpoint = "localhost:7001";
            var token = "Bearer SampleToken";
            var certificatePath = @"C:\Olympus\certificates\serverl5.crt";

            List<Interceptor> interceptors = new List<Interceptor>();
            List<Func<Metadata, Metadata>> functionInterceptors = new List<Func<Metadata, Metadata>>();

            //var client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>(endpoint, certificatePath, token, interceptors, functionInterceptors);
            var client = ClientHelper.GetInsecureClient<WebRtcStreamer.WebRtcStreamerClient>(endpoint, token, interceptors, functionInterceptors);

            var certificate = new X509Certificate2(certificatePath);

            Metadata metadata = new Metadata();
            metadata.Add(new Metadata.Entry("CertificateThumbprint", certificate.Thumbprint));
            metadata.Add(new Metadata.Entry("CertificateSubjectName", certificate.SubjectName.Name));
            
            //int val = 10;
            //var pingResponse = client.Ping(new PingRequest { Value = val });
            //Console.WriteLine($"Response: {pingResponse.Value}");

            //var privacy = client.GetPrivacy(new GetPrivacyRequest { StreamId = streamId });

            var actionResponse = await client.InitSessionAsync(new InitSessionRequest
            {
                AccessInfo = new AccessInfoMessage
                {
                    //TODO: AccessInfo accessInfo = new AccessInfo(Communications.GetFirstLocalAdapterIPv4Address(Communications.DefaultNetworkInterfaceName), Environment.UserName, "StatusBoard", Environment.MachineName, "Initialize webrtc stream");
                    ApplicationName = "Test",
                    Details = "Details",
                    Id = Guid.NewGuid().ToString(),
                    Ip = "127.0.0.1",

                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                },
                Quality = RxStreamQualityEnum.RxStreamQualityHdVideo,
                RouteToStreamingEncoder = true,
                StreamId = streamId,
                SessionId = sessionId,
                Offer = new WebRtcInfoMessage
                {
                    Aor = "AOR",
                    BypassMaxStreamRestrictions = true,
                    Type = string.Empty,
                    Message = message
                }
            }, metadata);

            var response = new CommandResponse()
            {
                OutputId = streamId,
                Messages = new List<string>()
            };

            foreach (var item in actionResponse.Answer)
            {
                response.Messages.Add(item.Message);
            }

            return response;
        }
    }
}
