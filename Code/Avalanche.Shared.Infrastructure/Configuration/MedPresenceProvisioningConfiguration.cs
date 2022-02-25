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
        public string Environment { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string CustomerName { get; set; }
        public string Department { get; set; }
        public string Specialty { get; set; }
    }

    public class EnvironmentSettingsConfiguration
    {
        public string ClientId { get; set; }
        public string ApiUrl { get; set; }
        public string IdentityUrl { get; set; }
    }
}
