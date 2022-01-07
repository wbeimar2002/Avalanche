using Ism.SystemState.Models.Medpresence;

namespace Avalanche.Api.ViewModels
{
    public class MedpresenceStateViewModel
    {
        public string? State { get; set; }
        public ulong? SessionId { get; set; }
        public bool IsRecording { get; set; }
        public int ImageCount { get; set; }
        public int ClipCount { get; set; }
        public MedpresenceAttendees? Attendees { get; set; }
    }
}
