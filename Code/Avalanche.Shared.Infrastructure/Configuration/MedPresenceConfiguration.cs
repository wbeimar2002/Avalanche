using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(MedPresenceConfiguration), 1)]
    public class MedPresenceConfiguration : IConfigurationPoco
    {
        public int Timeout { get; set; }

        public bool Validate()
        {
            var validator = new MedPresenceConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class MedPresenceConfigurationValidator : AbstractValidator<MedPresenceConfiguration>
        {
            public MedPresenceConfigurationValidator()
            {               
            }
        }
    }
}
