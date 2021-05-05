using Avalanche.Shared.Domain.Models.Media;

namespace Avalanche.Api.ViewModels
{
    public class DisplayRecordingViewModel
    {
        public SinkModel Display { get; set; }
        public RecordingChannelModel RecordChannel { get; set; }

        public bool Enabled { get; set; }

        public DisplayRecordingViewModel() { }

        public DisplayRecordingViewModel(SinkModel display, RecordingChannelModel recordChannel, bool enabled)
        {
            Display = display;
            RecordChannel = recordChannel;
            Enabled = enabled;
        }
    }
}
