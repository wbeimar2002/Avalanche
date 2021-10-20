using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.PrintServer.Client.V1;
using Ism.PrintServer.V1.Protos;
using Ism.Security.Grpc;
using static Ism.PrintServer.V1.Protos.PrintServer;

namespace Avalanche.Api.Services.Printing
{
    [ExcludeFromCodeCoverage]
    public class PrintingService : IPrintingService
    {
        private readonly PrintingServerSecureClient _printingService;

        public PrintingService(NamedServiceFactory<PrintingServerSecureClient, PrintServerClient> printServerFactory, PrintingConfiguration printingConfiguration)
        {
            if (printingConfiguration.UseVSSPrintingService)
                _printingService = printServerFactory.GetClient("PrintServerVSS");
            else
                _printingService = printServerFactory.GetClient("PrintServer");
        }

        public async Task<GetPrintersResponse> GetPrinters()
        {
            return await _printingService.GetPrinters(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
