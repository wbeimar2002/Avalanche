using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class RecorderConfiguration : IConfigurationPoco
    {
        public BackgroundRecordingMode BackgroundRecordingMode { get; set; }

        public bool Validate() => true;
    }
}
