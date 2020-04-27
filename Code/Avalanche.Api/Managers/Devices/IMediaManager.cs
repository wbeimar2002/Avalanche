using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface IMediaManager
    {
        Task<List<CommandResponseViewModel>> SendCommand(CommandViewModel command);
    }
}
