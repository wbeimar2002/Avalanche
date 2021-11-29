using System;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureImageViewModel
    {
        public Guid ImageId { get; set; }
        public string SourceName { get; set; }
        public string ChannelName { get; set; }
        public bool Is4k { get; set; }
        public string RelativePath { get; set; }
        public string Label { get; set; }

        public DateTimeOffset CaptureTimeUtc { get; set; }

        /// <summary>
        /// If the image was captured while recording, associate it with the video
        /// Will be null if the image was captured while the system was not recording
        /// </summary>
        public VideoReferenceViewModel? VideoReference { get; set; }

        /// <summary>
        /// If the image was captured while background recording, associate it with that video
        /// Will be null if background recording is disabled or if the image is captured from recorded video
        /// </summary>
        public VideoReferenceViewModel? BackgroundVideoReference { get; set; }

        public ProcedureImageViewModel()
        { }

        public ProcedureImageViewModel(Guid imageId, string sourceName, string channelName, bool is4k, string relativePath, VideoReferenceViewModel? videoReference,
            DateTimeOffset captureTimeUtc, string label)
        {
            ImageId = imageId;
            SourceName = sourceName;
            ChannelName = channelName;
            Is4k = is4k;
            RelativePath = relativePath;
            Label = label;
            VideoReference = videoReference;
            CaptureTimeUtc = captureTimeUtc;
        }
    }
}
