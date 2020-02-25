using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licensing
{
    public interface ILicensingManager
    {
        Task<bool> Validate(string key);
        Task<List<License>> GetAllActive();
    }
}
