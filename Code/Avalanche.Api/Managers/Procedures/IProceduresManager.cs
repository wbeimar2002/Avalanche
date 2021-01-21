using Avalanche.Api.ViewModels;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Procedures
{
    public interface IProceduresManager
    {
        Task<ActiveProcedureViewModel> GetActiveProcedure();

        Task ConfirmActiveProcedure();
    }
}