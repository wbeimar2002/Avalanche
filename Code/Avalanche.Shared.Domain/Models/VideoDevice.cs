using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalanche.Shared.Domain.Models
{
    /// <summary>
    /// Represents common properties shared between sources and sinks
    /// </summary>
    public class VideoDevice
    {
        /// <summary>
        /// AliasIndex used to identify this source/sink
        /// </summary>
        public AliasIndexApiModel Id { get; set; } = new AliasIndexApiModel();

        /// <summary>
        /// Friendly (display) name
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
    }
}
