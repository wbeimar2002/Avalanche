using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Google.Protobuf.WellKnownTypes;
using Ism.PrintServer.Client.V1;
using Ism.PrintServer.V1.Protos;
using Ism.Security.Grpc;
using Microsoft.FeatureManagement;
using static Ism.PrintServer.V1.Protos.PrintServer;

namespace Avalanche.Api.Services.Printing
{
    [ExcludeFromCodeCoverage]
    public class PrintingService : IPrintingService
    {
        private readonly PrintingServerSecureClient _printingService;

        public PrintingService(NamedServiceFactory<PrintingServerSecureClient, PrintServerClient> printServerFactory,
            IFeatureManager featureManager,
            PrintingConfiguration printingConfiguration)
        {
            var isDevice = featureManager.IsEnabledAsync(FeatureFlags.IsDevice).Result;

            if (isDevice)
            {
                if (printingConfiguration.UseVSSPrintingService)
                {
                    _printingService = printServerFactory.GetClient("Remote");
                }
                else
                {
                    _printingService = printServerFactory.GetClient("Local");
                }
            }
            else
            {
                _printingService = printServerFactory.GetClient("Local");
            }
        }

        public async Task<GetPrintersResponse> GetPrinters() =>
            await _printingService.GetPrinters(new Empty()).ConfigureAwait(false);
    }
}
