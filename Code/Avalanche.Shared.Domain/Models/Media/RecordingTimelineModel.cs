using System;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class RecordingTimelineModel
    {
        public Guid VideoId { get; set; }

        public TimeSpan VideoOffset { get; set; }
    }
}
