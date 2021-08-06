using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;


namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(LabelsConfiguration), 1)]
    public class LabelsConfiguration : IConfigurationPoco
    {
        public bool AutoLabelsEnabled { get; set; }

        public bool Validate()
        {
            var validator = new LabelsConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class LabelsConfigurationValidator : AbstractValidator<LabelsConfiguration>
        {
        }
    }
}
