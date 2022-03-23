namespace Avalanche.Shared.Domain.Models.Media
{
    public class HandleWebRtcMessageRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public WebRtcInfo Offer { get; set; } = new WebRtcInfo();
    }
}
