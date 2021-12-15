using System;

namespace Avalanche.Api.ViewModels
{
    public class VideoPositionReferenceViewModel
    {
        public TimeSpan OffsetFromVideoStart { get; set; }
        public string VideoId { get; set; }
    }
}
