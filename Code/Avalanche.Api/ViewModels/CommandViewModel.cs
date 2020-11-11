using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
