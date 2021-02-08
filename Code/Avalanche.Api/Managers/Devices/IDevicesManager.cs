using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface IDevicesManager
    {
        Task<List<CommandResponse>> SendCommand(CommandViewModel command, User user);
        
        Task<IList<VideoSource>> GetRoutingSources();
        Task<VideoSource> GetAlternativeSource(string alias, int index);
        Task<IList<VideoSink>> GetRoutingSinks();
    }
}
