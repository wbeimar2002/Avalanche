using Avalanche.Shared.Domain.Models.Media;

namespace Avalanche.Api.ViewModels
{
    public class DisplayRecordingRequestViewModel
    {
        public AliasIndexModel Display { get; set; }
        public RecordingChannelModel RecordChannel { get; set; }

        public bool Enabled { get; set; }

        public DisplayRecordingRequestViewModel() { }

        public DisplayRecordingRequestViewModel(AliasIndexModel display, RecordingChannelModel recordChannel, bool enabled)
        {
            Display = display;
            RecordChannel = recordChannel;
            Enabled = enabled;
        }
    }
}
