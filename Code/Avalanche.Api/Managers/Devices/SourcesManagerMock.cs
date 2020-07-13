using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    [ExcludeFromCodeCoverage]
    public class SourcesManagerMock : ISourcesManager
    {
        public Task<List<Source>> GetAllAvailable()
        {
            List<Source> sources = new List<Source>();
            sources.Add(new Source()
            {
                Name = "Charting System",
            });

            sources.Add(new Source()
            {
                Name = "Nurse PC",
            });

            sources.Add(new Source()
            {
                Name = "Phys. PC",
            });

            return Task.FromResult(sources);
        }
    }
}
