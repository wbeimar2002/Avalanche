namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class FeaturesConfiguration
    {
        public bool IsVSS => !IsDevice;

        public bool IsDevice { get; set; }
        public bool ActiveProcedure { get; set; }
        public bool Devices { get; set; }
        public bool Media { get; set; }
        public bool Presets { get; set; }
        public bool Recording { get; set; }
        public bool WebRtc { get; set; }
    }
}
