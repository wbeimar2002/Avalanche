using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class CommandViewModel
    {
        public CommandTypes CommandType { get; set; }

        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }

        public List<Device> Devices { get; set; }
        public List<Device> Destinations { get; set; }
        public User User { get; set; }
    }
}
