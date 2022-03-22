using System.Collections.Generic;
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
}
