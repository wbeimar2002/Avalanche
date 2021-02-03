using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class PgsTimeoutConfig
    {
        /// <summary>
        /// Id of the video source to use for PGS
        /// </summary>
        public AliasIndexApiModel PgsSource { get; set; }

        /// <summary>
        /// List of displays to route PGS to
        /// </summary>
        public List<AliasIndexApiModel> PgsSinks { get; set; }

        /// <summary>
        /// Id of the video source to use for timeout
        /// If timeout is a pdf file, this should be the same as the PGS source
        /// If not, it is typically nurse pc
        /// </summary>
        public AliasIndexApiModel TimeoutSource { get; set; }

        /// <summary>
        /// Is timeout a pdf file or video source?
        /// </summary>
        public TimeoutMode TimeoutMode { get; set; }

        /// <summary>
        /// List of displays to send timeout to
        /// </summary>
        public List<AliasIndexApiModel> TimeoutSinks { get; set; }
    }
}
