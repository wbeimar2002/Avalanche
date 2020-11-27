using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licensing
{
    public interface ILicensingManager
    {
        Task<bool> Validate(string key);
        Task<List<License>> GetAllActive();
    }
}
