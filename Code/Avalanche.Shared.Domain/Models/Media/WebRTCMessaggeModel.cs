using System.Collections.Generic;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class WebRTCMessaggeModel_OLD
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public class WebRtcInfo
    {
        public string AoR { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool BypassMaxStreamRestrictions { get; set; }
    }


    public class InitWebRtcSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string StreamName { get; set; } = string.Empty;

        public WebRtcInfo Offer { get; set; } = new WebRtcInfo();

        public string ExternalIp { get; set; } = string.Empty;
        public string RemoteUser { get; set; } = string.Empty;
        public string RemoteIp { get; set; } = string.Empty;
    }

    public class InitWebRtcSessionResponse
    {
        public IList<WebRtcInfo> Answer { get; set; } = new List<WebRtcInfo>();
    }

    public class HandleWebRtcMessageRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public WebRtcInfo Offer { get; set; } = new WebRtcInfo();
    }

    public class DeInitWebRtcSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public class GetWebRtcStreamsResponse
    {
        public IList<string> StreamNames { get; set; } = new List<string>();
    }
}
