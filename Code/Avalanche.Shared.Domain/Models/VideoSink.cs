using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class VideoSink : VideoDevice
    {
        /// <summary>
        /// Video source routed to this display. No video means an empty alias
        /// </summary>
        public AliasIndexApiModel Source { get; set; } = new AliasIndexApiModel();

        /// <summary>
        /// Does this display support tiling?
        /// Should always be false for now, will need this for RX
        /// </summary>
        public bool TilingEnabled { get; set; } = false;
    }
}
