using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ism.PrintServer.Client.V1;
using Ism.PrintServer.V1.Protos;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Services.Printing
{
    [ExcludeFromCodeCoverage]
    public class PrintingService : IPrintingService
    {
        private readonly PrintingServerSecureClient _client;

        // TODO: inject local vs vss print client
        public PrintingService(PrintingServerSecureClient client) => _client = ThrowIfNullOrReturn(nameof(client), client);

        public async Task<GetPrintersResponse> GetPrinters() => await _client.GetPrinters(new Google.Protobuf.WellKnownTypes.Empty()).ConfigureAwait(false);
    }
}
