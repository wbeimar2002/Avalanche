using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Command
    {
        public Device Device { get; set; }
        public List<Device> Destinations { get; set; }
        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public AccessInfo AccessInformation { get; set; }
        public User User { get; set; }
    }
}
