using System.Collections.Generic;
using Avalanche.Shared.Domain.Models.Media;
using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;
using Ism.Common.Core.Configuration.Extensions;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(WebRtcApiConfiguration), 1)]
    public class WebRtcApiConfiguration : IConfigurationPoco
    {
        /// <summary>
        /// Stream name to use for WebRtc preview, on BX it should be "BX4Preview"
        /// </summary>
        public string PreviewStreamName { get; set; } = string.Empty;

        /// <summary>
        /// Configures source selection. Used by MP, LSP, and maybe RoomLink
        /// </summary>
        public SourceSelectionConfiguration SourceSelectionConfiguration { get; set; } = new SourceSelectionConfiguration();

        /// <summary>
        /// Collection of streams that should be shown in MP, LSP, etc
        /// Note that "private" streams such as "BX4Preview" won't be returned by this list
        /// </summary>
        public List<ViewableStreamConfiguration> ViewableStreams { get; set; } = new List<ViewableStreamConfiguration>();

        public bool Validate()
        {
            SourceSelectionConfiguration.Validate();
            ViewableStreams.ValidateAll();

            return true;
        }
    }

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
