using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class InitWebRtcSessionResponse
    {
        public IList<WebRtcInfo> Answer { get; set; } = new List<WebRtcInfo>();
    }
}
