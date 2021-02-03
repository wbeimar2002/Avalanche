using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Command
    {
        public VideoDevice Device { get; set; }
        public List<VideoDevice> Destinations { get; set; }
        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public AccessInfo AccessInformation { get; set; }
        public User User { get; set; }
    }
}
