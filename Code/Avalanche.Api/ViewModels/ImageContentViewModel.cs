using System;

namespace Avalanche.Api.ViewModels
{
    public class ImageContentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTimeOffset CaptureTimeUtc { get; set; }
        public string Thumbnail { get; set; }
    }
}
