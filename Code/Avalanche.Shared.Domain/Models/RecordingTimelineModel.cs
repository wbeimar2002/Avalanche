using System;

namespace Avalanche.Shared.Domain.Models
{
    public class RecordingTimelineModel
    {
        public Guid VideoId { get; set; }

        public TimeSpan VideoOffset { get; set; }
    }
}
