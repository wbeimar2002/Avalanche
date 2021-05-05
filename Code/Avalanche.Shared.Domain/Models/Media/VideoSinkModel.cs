namespace Avalanche.Shared.Domain.Models.Media
{
    public class VideoSinkModel : VideoDeviceModel
    {
        /// <summary>
        /// Video source routed to this display. No video means an empty alias
        /// </summary>
        public AliasIndexModel Source { get; set; } = new AliasIndexModel();

        /// <summary>
        /// Does this display support tiling?
        /// Should always be false for now, will need this for RX
        /// </summary>
        public bool TilingEnabled { get; set; } = false;
    }
}
