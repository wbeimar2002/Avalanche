using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers.Data
{
    public interface IPhysiciansManager
    {
        Task<IList<PhysicianViewModel>> GetPhysicians();
        Task<IList<PhysicianSearchResultViewModel>> GetPhysicians(string keyword);
    }
}
