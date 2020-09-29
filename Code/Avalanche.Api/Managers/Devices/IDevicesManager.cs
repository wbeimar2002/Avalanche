using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface IDevicesManager
    {
        Task SendCommandAsync(CommandViewModel command);
        Task<IList<Source>> GetOperationsSources();
        Task<IList<Output>> GetOperationsOutputs();
        Task<List<Output>> GetPGSOutputs();
        Task<List<Output>> GetTimeoutOutputs();
    }
}
