using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(AutoLoginConfiguration), 1)]
    public class AutoLoginConfiguration : IConfigurationPoco
    {
        public bool AutoLogin { get; set; }

        public bool Validate() => true;
    }
}
