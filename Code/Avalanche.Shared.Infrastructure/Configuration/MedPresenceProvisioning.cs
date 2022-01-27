using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(MedPresenceProvisioningConfiguration), 1)]
    public class MedPresenceProvisioningConfiguration : IConfigurationPoco
    {
        public InputParametersConfiguration InputParameters { get; set; }
        public EnvironmentSettingsConfiguration EnvironmentSettings { get; set; }

        public bool Validate()
        {
            var validator = new MedPresenceProvisioningConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class MedPresenceProvisioningConfigurationValidator : AbstractValidator<MedPresenceProvisioningConfiguration>
        {
            public MedPresenceProvisioningConfigurationValidator()
            {               
            }
        }
    }

    public class InputParametersConfiguration
    {
        public int Timeout { get; set; }
        public int Duration { get; set; }
    }

    public class EnvironmentSettingsConfiguration
    {
        public int Timeout { get; set; }
        public int Duration { get; set; }
    }
}
