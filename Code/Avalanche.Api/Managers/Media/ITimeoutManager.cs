using Avalanche.Api.ViewModels;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface ITimeoutManager
    {
        Task<string> GetTimeoutPdfPath();
        Task<int> GetTimeoutPageCount();
        Task<int> GetTimeoutPage();
        Task SetTimeoutPage(int pageNumber);
        Task NextPage();
        Task PreviousPage();
        Task StartTimeout();
        Task StopTimeout(bool restoreLastRoutes);
        Task DeActivateTimeout();
    }
}
