using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licencing
{
    public interface ILicensingManager
    {
        Task<bool> Validate(string key);        
    }
}
