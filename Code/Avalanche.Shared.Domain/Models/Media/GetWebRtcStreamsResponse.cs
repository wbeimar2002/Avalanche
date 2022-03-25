using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class GetWebRtcStreamsResponse
    {
        public IList<string> StreamNames { get; set; } = new List<string>();
    }
}
