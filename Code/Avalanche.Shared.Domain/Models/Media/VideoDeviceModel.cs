using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models.Media
{
    /// <summary>
    /// Represents common properties shared between sources and sinks
    /// </summary>
    public class VideoDeviceModel
    {
        /// <summary>
        /// Used to identify this source/sink
        /// </summary>
        public SinkModel? Sink { get; set; }

        /// <summary>
        /// Friendly (display) name
        /// </summary>
        public string? Name { get; set; }

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
        public string? Type { get; set; }
    }
}
