namespace Avalanche.Shared.Domain.Models.Media
{
    public class WebRtcInfo
    {
        public string AoR { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool BypassMaxStreamRestrictions { get; set; }
    }
}
