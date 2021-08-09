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

        public TimeSpan? CaptureOffsetFromVideoStart { get; set; }
        public DateTimeOffset CaptureTimeUtc { get; set; }

        public ProcedureImageViewModel() 
        { }

        public ProcedureImageViewModel(Guid imageId, string sourceName, string channelName, bool is4k, string relativePath, TimeSpan? captureOffsetFromVideoStart,
            DateTimeOffset captureTimeUtc, string label)
        {
            ImageId = imageId;
            SourceName = sourceName;
            ChannelName = channelName;
            Is4k = is4k;
            RelativePath = relativePath;
            Label = label;
            CaptureOffsetFromVideoStart = captureOffsetFromVideoStart;
            CaptureTimeUtc = captureTimeUtc;
        }
    }
}
