using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Data
{
    public interface IPhysiciansManager
    {
        Task<List<dynamic>> GetPhysicians();
    }
}
