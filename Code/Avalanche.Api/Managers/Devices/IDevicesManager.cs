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
        Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command);
        Task<List<Device>> GetOperationsSources();
        Task<List<Device>> GetOperationsDestinations();
        Task<List<Device>> GetSetupDestinations();
        Task<List<Device>> GetTimeoutDestinations();
    }
}
