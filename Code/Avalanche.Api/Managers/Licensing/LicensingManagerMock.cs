using AutoFixture;
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Licensing
{
    [ExcludeFromCodeCoverage]
    public class LicensingManagerMock : ILicensingManager
    {
        public Task<List<LicenseModel>> GetAllActive()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<LicenseModel>(10).ToList());
        }

        public Task<bool> Validate(string key) => Task.FromResult(true);
    }
}
