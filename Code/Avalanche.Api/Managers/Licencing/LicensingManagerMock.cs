using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licencing
{
    public class LicensingManagerMock : ILicensingManager
    {
        public Task<bool> Validate(string key)
        {
            return Task.FromResult(true);
        }
    }
}
