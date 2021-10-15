using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class MedpresenceStateViewModel
    {
        public string? State { get; set; }
        public ulong? SessionId { get; set; }
        public bool IsRecording { get; set; }
        public int ImageCount { get; set; }
        public int ClipCount { get; set; }
        public List<string>? Attendees { get; set; }
    }
}
