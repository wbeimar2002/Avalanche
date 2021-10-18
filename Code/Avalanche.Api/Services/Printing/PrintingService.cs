using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.PrintServer.Client;
using Ism.PrintServer.Client.V1;

namespace Avalanche.Api.Services.Printing
{
    [ExcludeFromCodeCoverage]
    public class PrintingService : IPrintingService
    {
        private readonly PrintingServerSecureClient _printingService;

        public PrintingService(PrintServerFactory printServerFactory, PrintingConfiguration printingConfiguration)
        {
            if (printingConfiguration.UseVSSPrintingService)
                _printingService = printServerFactory.GetClient("PrintServerVSS");
            else
                _printingService = printServerFactory.GetClient("PrintServer");
        }

        public async Task<PrintersResponse> GetPrinters()
        {
            return await _printingService.GetPrinters(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
