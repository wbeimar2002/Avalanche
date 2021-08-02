using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(GeneralApiConfiguration), 1)]
    public class GeneralApiConfiguration : IConfigurationPoco
    {
        public int CacheDuration { get; set; }
        public bool AdHocLabelsAllowed { get; set; }

        public bool Validate()
        {
            var validator = new GeneralApiConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class GeneralApiConfigurationValidator : AbstractValidator<GeneralApiConfiguration>
        {
            public GeneralApiConfigurationValidator()
            {               
            }
        }
    }
}
