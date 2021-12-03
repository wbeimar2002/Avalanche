using AvidisDeviceInterface.Client.V1;
using AvidisDeviceInterface.V1.Protos;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class AvidisService : IAvidisService
    {
        private readonly AvidisSecureClient _client;

        public AvidisService(AvidisSecureClient client) => _client = ThrowIfNullOrReturn(nameof(client), client);

        public async Task HidePreview(HidePreviewRequest hidePreviewRequest) => await _client.HidePreview(hidePreviewRequest).ConfigureAwait(false);

        public async Task ShowPreview(ShowPreviewRequest showPreviewRequest) => await _client.ShowPreview(showPreviewRequest).ConfigureAwait(false);

        public async Task<GetGpioPinsResponse> GetGpioPins() => await _client.GetGpioPins(new Empty()).ConfigureAwait(false);        
    }
}
