using System.Threading.Tasks;
using Ism.PrintServer.Common.Core;

namespace Avalanche.Api.Services.Printing
{
    public interface IPrintingService
    {
        Task<PrintersResponse> GetPrinters();
    }
}
