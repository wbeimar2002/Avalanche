using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models.Configuration
{
    public class PgsTimeoutConfig
    {
        /// <summary>
        /// Id of the video source to use for PGS
        /// </summary>
        public SinkModel PgsSource { get; set; }

        /// <summary>
        /// List of displays to route PGS to
        /// </summary>
        public List<SinkModel> PgsSinks { get; set; }

        /// <summary>
        /// Id of the video source to use for timeout
        /// If timeout is a pdf file, this should be the same as the PGS source
        /// If not, it is typically nurse pc
        /// </summary>
        public SinkModel TimeoutSource { get; set; }

        /// <summary>
        /// Is timeout a pdf file or video source?
        /// </summary>
        public TimeoutModes TimeoutMode { get; set; }

        /// <summary>
        /// List of displays to send timeout to
        /// </summary>
        public List<SinkModel> TimeoutSinks { get; set; }
    }
}
