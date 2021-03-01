using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Enumerations
{
    /// <summary>
    /// Mode of operation for timeout
    /// </summary>
    public enum TimeoutModes
    {
        /// <summary>
        /// Timeout is a pdf file
        /// </summary>
        PdfFile = 0,

        /// <summary>
        /// Timeout is a fullscreen video source, typically nurse pc
        /// </summary>
        VideoSource = 1
    }
}
