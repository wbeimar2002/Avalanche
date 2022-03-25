using Avalanche.Shared.Domain.Models.Media;
using FluentValidation;
using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class SourceSelectionConfiguration : IConfigurationPoco
    {
        /// <summary>
        /// Is source selection enabled?
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// WebRtc stream name, such as "BX4StreamB" which is the source selection stream
        /// </summary>
        public string StreamName { get; set; } = string.Empty;

        /// <summary>
        /// Avidis sink to route video to for source selection
        /// </summary>
        public AliasIndexModel VideoSink { get; set; } = new AliasIndexModel();

        public bool Validate()
        {
            if (!Enabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(StreamName))
            {
                throw new ValidationException("Source selection is enabled but has an empty stream name");
            }

            if (VideoSink.IsEmpty())
            {
                throw new ValidationException("Source selection is enabled but has an empty routing sink");
            }

            return true;
        }
    }
}
