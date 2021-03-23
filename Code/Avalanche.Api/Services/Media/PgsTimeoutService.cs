using Ism.PgsTimeout.Client.V1;
using Ism.PgsTimeout.V1.Protos;

using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class PgsTimeoutService : IPgsTimeoutService
    {
        private readonly PgsTimeoutSecureClient _client;

        public PgsTimeoutService(PgsTimeoutSecureClient client)
        {
            _client = client;
        }

        public async Task<GetPgsMuteResponse> GetPgsMute() => await _client.GetPgsMute();

        public async Task<GetPgsPlaybackStateResponse> GetPgsPlaybackState() => await _client.GetPgsPlaybackState();

        public async Task<GetPgsTimeoutModeResponse> GetPgsTimeoutMode() => await _client.GetPgsTimeoutMode();
        public async Task<GetPgsVideoFileResponse> GetPgsVideoFile() => await _client.GetPgsVideoFile();

        public async Task<GetPgsVideoListResponse> GetPgsVideoFileList() => await _client.GetPgsVideoFileList();

        public async Task<GetPgsVolumeResponse> GetPgsVolume() => await _client.GetPgsVolume();

        public async Task<GetTimeoutPageResponse> GetTimeoutPage() => await _client.GetTimeoutPage();

        public async Task<GetTimeoutPageCountResponse> GetTimeoutPageCount() => await _client.GetTimeoutPageCount();

        public async Task<GetTimeoutPdfPathResponse> GetTimeoutPdfPath() => await _client.GetTimeoutPdfPath();

        public async Task NextPage() => await _client.NextPage();

        public async Task PreviousPage() => await _client.PreviousPage();

        public async Task SetPgsMute(SetPgsMuteRequest request) => await _client.SetPgsMute(request);

        public async Task SetPgsPlaybackState(SetPgsPlaybackStateRequest request) => await _client.SetPgsPlaybackState(request);

        public async Task SetPgsTimeoutMode(SetPgsTimeoutModeRequest request) => await _client.SetPgsTimeoutMode(request);
        public async Task SetPgsVideoFile(SetPgsVideoFileRequest request) => await _client.SetPgsVideoFile(request);

        public async Task SetPgsVideoPosition(SetPgsVideoPositionRequest request) => await _client.SetPgsVideoPosition(request);

        public async Task SetPgsVolume(SetPgsVolumeRequest request) => await _client.SetPgsVolume(request);
        public async Task SetTimeoutPage(SetTimeoutPageRequest request) => await _client.SetTimeoutPage(request);
    }
}
