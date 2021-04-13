using Avalanche.Api.ViewModels;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface ITimeoutManager
    {
        Task<StateViewModel> GetTimeoutPdfPath();
        Task<StateViewModel> GetTimeoutPageCount();
        Task<StateViewModel> GetTimeoutPage();
        Task SetTimeoutPage(StateViewModel requestViewModel);
        Task NextPage();
        Task PreviousPage();
        Task SetTimeoutState(StateViewModel requestViewModel);
    }
}
