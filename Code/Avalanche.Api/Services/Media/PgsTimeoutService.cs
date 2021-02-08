using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.PgsTimeout.Client.V1;
using Ism.Security.Grpc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using Ism.PgsTimeout.V1.Protos;

namespace Avalanche.Api.Services.Media
{
    public class PgsTimeoutService : IPgsTimeoutService
    {
        private readonly IConfigurationService _configurationService;
        private readonly PgsTimeoutSecureClient _client;

        public PgsTimeoutService(
            IConfigurationService configurationService, 
            IGrpcClientFactory<PgsTimeoutClient> grpcClientFactory, 
            ICertificateProvider certificateProvider)
        {
            _configurationService = ThrowIfNullOrReturn(nameof(configurationService), configurationService);
            ThrowIfNull(nameof(grpcClientFactory), grpcClientFactory);
            ThrowIfNull(nameof(certificateProvider), certificateProvider);

            var ip = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var port = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");

            _client = new PgsTimeoutSecureClient(grpcClientFactory, ip, port, certificateProvider);
        }

        public async Task<GetPgsTimeoutModeResponse> GetPgsTimeoutMode() => await _client.GetPgsTimeoutMode();
        public async Task SetPgsTimeoutMode(SetPgsTimeoutModeRequest request) => await _client.SetPgsTimeoutMode(request);

        public async Task<GetPgsVolumeResponse> GetPgsVolume() => await _client.GetPgsVolume();
        public async Task SetPgsVolume(SetPgsVolumeRequest request) => await _client.SetPgsVolume(request);

        public async Task<GetPgsMuteResponse> GetPgsMute() => await _client.GetPgsMute();
        public async Task SetPgsMute(SetPgsMuteRequest request) => await _client.SetPgsMute(request);

        public async Task<GetPgsPlaybackStateResponse> GetPgsPlaybackState() => await _client.GetPgsPlaybackState();
        public async Task SetPgsPlaybackState(SetPgsPlaybackStateRequest request) => await _client.SetPgsPlaybackState(request);

        public async Task<GetPgsVideoListResponse> GetPgsVideoFileList() => await _client.GetPgsVideoFileList();
        public async Task<GetPgsVideoFileResponse> GetPgsVideoFile() => await _client.GetPgsVideoFile();
        public async Task SetPgsVideoFile(SetPgsVideoFileRequest request) => await _client.SetPgsVideoFile(request);

        public async Task SetPgsVideoPosition(SetPgsVideoPositionRequest request) => await _client.SetPgsVideoPosition(request);



        public async Task<GetTimeoutPdfPathResponse> GetTimeoutPdfPath() => await _client.GetTimeoutPdfPath();
        public async Task<GetTimeoutPageCountResponse> GetTimeoutPageCount() => await _client.GetTimeoutPageCount();
        public async Task<GetTimeoutPageResponse> GetTimeoutPage() => await _client.GetTimeoutPage();
        public async Task SetTimeoutPage(SetTimeoutPageRequest request) => await _client.SetTimeoutPage(request);
        public async Task NextPage() => await _client.NextPage();
        public async Task PreviousPage() => await _client.PreviousPage();

    }
}
