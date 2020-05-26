using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface ISourcesManager
    {
        Task<List<Source>> GetAllAvailable();
    }
}
