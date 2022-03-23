using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class GetLiveStreamsResponse
    {
        public AliasIndexModel SourceSelectionSink { get; set; } = new AliasIndexModel();

        public string SourceSelectionStreamName { get; set; } = string.Empty;

        public IList<LiveStreamInfo> Streams { get; set; } = new List<LiveStreamInfo>();
    }
}
