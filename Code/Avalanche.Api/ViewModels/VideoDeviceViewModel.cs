using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    [Obsolete("Decompose into smaller viewmodels for sinks and sources")]
    public class VideoDeviceViewModel
    {
        public AliasIndexViewModel Id { get; set; }

        public string Name { get; set; }

        public string Preview { get; set; }

        public bool HasSignal { get; set; }


        /// <summary>
        /// Should this source/display show up in the UI?
        /// For example, an EZport source would not show up in the UI, but it does need to be configured
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Button index, also determines group A/B
        /// </summary>
        public int PositionInScreen { get; set; }

        /// <summary>
        /// UI type, which icon to use for sources and which button graphic to use for displays
        /// </summary>
        public string Type { get; set; } = string.Empty;


        /// <summary>
        /// Does this video source have video?
        /// </summary>
        public bool HasVideo { get; set; }

        /// <summary>
        /// Does this video source change at runtime?
        /// For now, EZport sources and dp dongle sources would use this
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// What contextual controls should this source have?
        /// For example, a movable camera would have this set as "PTZ" and the UI would show directional controls
        /// Empty for now
        /// </summary>
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// Does this display support tiling?
        /// Should always be false for now, will need this for RX
        /// </summary>
        public bool TilingEnabled { get; set; } = false;
    }
}
