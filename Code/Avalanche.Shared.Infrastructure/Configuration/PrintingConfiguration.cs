using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;


namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(PrintingConfiguration), 1)]
    public class PrintingConfiguration : IConfigurationPoco
    {
        public bool AllowExportToUSB { get; set; }
        public bool VssPrint { get; set; }

        public bool Validate()
        {
            var validator = new PrintingConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class PrintingConfigurationValidator : AbstractValidator<PrintingConfiguration>
        { }
    }
}
