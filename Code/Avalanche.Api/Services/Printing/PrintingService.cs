using System.Threading.Tasks;
using Ism.PrintServer.Client.V1;
using Ism.PrintServer.Common.Core;

namespace Avalanche.Api.Services.Printing
{
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
