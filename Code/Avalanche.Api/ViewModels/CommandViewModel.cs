using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class CommandViewModel
    {
        public CommandTypes CommandType { get; set; }

        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }

        public List<VideoDevice> Devices { get; set; }
        public List<VideoDevice> Destinations { get; set; }
        public User User { get; set; }
    }
}
