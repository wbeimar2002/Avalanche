using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.PgsTimeout.Common.Core;
using Ism.Security.Grpc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        readonly IConfigurationService _configurationService;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly string _hostIpAddress;

        public Ism.Streaming.V1.Protos.WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }

        public MediaService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);
            UseMocks = true;

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{mediaServiceGrpcPort}", certificate);
            WebRtcStreamerClient = ClientHelper.GetInsecureClient<Ism.Streaming.V1.Protos.WebRtcStreamer.WebRtcStreamerClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
            PgsTimeoutClient = ClientHelper.GetInsecureClient<PgsTimeout.PgsTimeoutClient>($"https://{_hostIpAddress}:{mediaServiceGrpcPort}");
        }

        #region WebRTC

        public async Task<Ism.Streaming.V1.Protos.GetSourceStreamsResponse> GetSourceStreamsAsync()
        {
            return await WebRtcStreamerClient.GetSourceStreamsAsync(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public async Task HandleMessageAsync(Ism.Streaming.V1.Protos.HandleMessageRequest handleMessageRequest)
        {
            return await WebRtcStreamerClient.HandleMessageAsync(handleMessageRequest);
        }

        public async Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(Ism.Streaming.V1.Protos.InitSessionRequest initSessionRequest)
        {
            return await WebRtcStreamerClient.InitSessionAsync(initSessionRequest);
        }

        public async Task DeInitSessionAsync(Ism.Streaming.V1.Protos.DeInitSessionRequest deInitSessionRequest)
        {
            return await WebRtcStreamerClient.DeInitSessionAsync(deInitSessionRequest);
        }

        #endregion WebRTC
    }
}
