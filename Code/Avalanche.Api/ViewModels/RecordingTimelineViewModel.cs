using System;

namespace Avalanche.Api.ViewModels
{
    public class RecordingTimelineViewModel
    {
        public Guid VideoId { get; set; }

        public TimeSpan VideoOffset { get; set; }
    }
}
