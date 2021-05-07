﻿namespace Avalanche.Shared.Domain.Models.Media
{
    public class RecordingChannelModel
    {
        public string? ChannelName { get; set; }
        public AliasIndexModel? VideoSink { get; set; }
        public bool Is4k { get; set; }
    }
}
