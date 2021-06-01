using Avalanche.Shared.Domain.Models.Media;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class DisplayRecordingViewModel
    {
        public AliasIndexModel Display { get; set; }
        public List<RecordingChannelModel> RecordChannels { get; set; }

        public bool Enabled { get; set; }

        public DisplayRecordingViewModel() { }

        public DisplayRecordingViewModel(AliasIndexModel display, List<RecordingChannelModel> recordChannels, bool enabled)
        {
            Display = display;
            RecordChannels = recordChannels;
            Enabled = enabled;
        }
    }
}
