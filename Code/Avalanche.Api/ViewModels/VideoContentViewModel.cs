using System;

namespace Avalanche.Api.ViewModels
{
    public class VideoContentViewModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public DateTimeOffset CaptureTimeUtc { get; set; }
        public string Thumbnail { get; set; }
        public string Stream { get; set; }
        public string Label { get; set; }
        public VideoPositionReferenceViewModel BackgroundVideoReference { get; set; }
        public int? Length { get; set; }
    }
}
