using System;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureVideoViewModel
    {
        public Guid VideoId { get; set; }
        public string SourceName { get; set; }
        public string ChannelName { get; set; }
        public string RelativePath { get; set; }
        public string Label { get; set; }
        public string ThumbnailRelativePath { get; set; }
        public DateTimeOffset VideoStartTimeUtc { get; set; }
        public DateTimeOffset? VideoStopTimeUtc { get; set; }
        public TimeSpan VideoDuration { get; set; }

        public ProcedureVideoViewModel()
        { }

        public ProcedureVideoViewModel(Guid videoId, string sourceName, string channelName, string relativePath, string thumbnailRelativePath, DateTimeOffset videoStartTimeUtc, DateTimeOffset? videoStopTimeUtc, TimeSpan videoDuration, string label)
        {
            VideoId = videoId;
            SourceName = sourceName;
            ChannelName = channelName;
            RelativePath = relativePath;
            ThumbnailRelativePath = thumbnailRelativePath;
            Label = label;
            VideoStartTimeUtc = videoStartTimeUtc;
            VideoStopTimeUtc = videoStopTimeUtc;
            VideoDuration = videoDuration;
        }
    }
}
