using AvidisDeviceInterface.Client.V1;
using AvidisDeviceInterface.V1.Protos;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class AvidisService : IAvidisService
    {
        private readonly AvidisSecureClient _client;

        public AvidisService(AvidisSecureClient client)
        {
            _client = client;
        }

        public async Task HidePreview(HidePreviewRequest hidePreviewRequest)
        {
            await _client.HidePreview(hidePreviewRequest);
        }

        public async Task ShowPreview(ShowPreviewRequest showPreviewRequest)
        {
            await _client.ShowPreview(showPreviewRequest);
        }
    }
}
