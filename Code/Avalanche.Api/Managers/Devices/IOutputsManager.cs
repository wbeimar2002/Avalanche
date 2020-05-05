using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface IOutputsManager
    {
        Task<List<Output>> GetAllAvailable();
        Task<State> GetCurrentState(string id, StateTypes stateType);
        Task<Content> GetContent(string contentType);
        Task<List<State>> GetCurrentStates(string id);
    }
}
