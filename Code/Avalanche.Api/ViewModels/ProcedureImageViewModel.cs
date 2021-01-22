using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureImageViewModel
    {
        public Guid ImageId { get; set; }
        public string SourceName { get; set; }
        public string ChannelName { get; set; }
        public bool Is4k { get; set; }
        public string ImagePath { get; set; }

        public TimeSpan? CaptureOffsetFromVideoStart { get; set; }
        public DateTimeOffset CaptureTime { get; set; }

        public ProcedureImageViewModel() 
        { }
        public ProcedureImageViewModel(Guid imageId, string sourceName, string channelName, bool is4k, string imagePath, TimeSpan? captureOffsetFromVideoStart, DateTimeOffset captureTime)
        {
            ImageId = imageId;
            SourceName = sourceName;
            ChannelName = channelName;
            Is4k = is4k;
            ImagePath = imagePath;
            CaptureOffsetFromVideoStart = captureOffsetFromVideoStart;
            CaptureTime = captureTime;
        }
    }
}
