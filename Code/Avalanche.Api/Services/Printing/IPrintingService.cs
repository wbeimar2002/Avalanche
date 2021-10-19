using System.Threading.Tasks;
using Ism.PrintServer.V1.Protos;

namespace Avalanche.Api.Services.Printing
{
    public interface IPrintingService
    {
        Task<GetPrintersResponse> GetPrinters();
    }
}
