using AutoFixture;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licensing
{
    public class LicensingManagerMock : ILicensingManager
    {
        public Task<List<License>> GetAllActive()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<License>(10).ToList());
        }

        public Task<bool> Validate(string key)
        {
            return Task.FromResult(true);
        }
    }
}
