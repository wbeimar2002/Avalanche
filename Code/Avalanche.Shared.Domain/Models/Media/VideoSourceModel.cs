using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class VideoSourceModel : VideoDeviceModel
    {
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
    }
}
