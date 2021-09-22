using System.Threading.Tasks;
using Ism.PrintServer.Client;

namespace Avalanche.Api.Services.Printing
{
    public interface IPrintingService
    {
        Task<PrintersResponse> GetPrinters();
    }
}
