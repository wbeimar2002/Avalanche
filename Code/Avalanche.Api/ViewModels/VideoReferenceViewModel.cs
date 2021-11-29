using System;

namespace Avalanche.Api.ViewModels
{
    public class VideoReferenceViewModel
    {
        /// <summary>
        /// If the image is captured while background recording, this is the offset from that video.
        /// </summary>
        public TimeSpan OffsetFromVideoStart { get; set; }

        /// <summary>
        /// If this video is recorded while background recording is active, it will be the <see cref="VideoId"/> of the background video
        /// </summary>
        public Guid VideoId { get; set; }

        // make the serializer happy
        public VideoReferenceViewModel() { }

        public VideoReferenceViewModel(TimeSpan offsetFromVideoStart, Guid videoId) => (OffsetFromVideoStart, VideoId) = (offsetFromVideoStart, videoId);
    }
}
