using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ism.PrintServer.Client;
using Ism.PrintServer.Client.V1;

namespace Avalanche.Api.Services.Printing
{
    [ExcludeFromCodeCoverage]
    public class PrintingService : IPrintingService
    {
        private readonly PrintingServerSecureClient _printingService;

        public PrintingService(PrintingServerSecureClient printingService)
        {
            _printingService = printingService;
        }

        public async Task<PrintersResponse> GetPrinters()
        {
            return await _printingService.GetPrinters(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
