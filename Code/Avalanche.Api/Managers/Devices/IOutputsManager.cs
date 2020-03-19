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
        Task<State> GetCurrentState(string id, StateTypes commandType);
        Task SendCommand(string id, Command command);
        Task<Signal> GetContent(string contentType);
    }
}
