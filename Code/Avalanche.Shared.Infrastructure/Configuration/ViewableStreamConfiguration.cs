using FluentValidation;
using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class ViewableStreamConfiguration : IConfigurationPoco
    {
        /// <summary>
        /// Should match one of the stream name(s) in Media's MediaSourceConfiguration list of sources
        /// </summary>
        public string StreamName { get; set; } = string.Empty;

        /// <summary>
        /// Optional audio stream name. If not empty, the audio/video will be streamed together
        /// </summary>
        public string AudioStreamName { get; set; } = string.Empty;

        /// <summary>
        /// User facing display name of this stream such as "Fixed Cam" or "Channel A"
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(StreamName))
            {
                throw new ValidationException("Viewable stream has an empty stream name");
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                throw new ValidationException("Viewable stream has an empty display name");
            }

            // audio stream name is optional
            // if it's empty, the stream will be video only

            return true;
        }
    }
}
