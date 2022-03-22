namespace Avalanche.Shared.Domain.Models.Media
{
    public class InitWebRtcSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string StreamName { get; set; } = string.Empty;

        public WebRtcInfo Offer { get; set; } = new WebRtcInfo();

        public string ExternalIp { get; set; } = string.Empty;
        public string RemoteUser { get; set; } = string.Empty;
        public string RemoteIp { get; set; } = string.Empty;
    }
}
