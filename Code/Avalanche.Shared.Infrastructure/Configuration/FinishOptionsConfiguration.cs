using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(FinishOptionsConfiguration), 1)]
    public class FinishOptionsConfiguration : IConfigurationPoco
    {
        public bool IncludePHI { get; set; }
        public bool Print { get; set; }
        public bool AllowExportToUSB { get; set; }
        public int Printer { get; set; }
        public string PaperOrientation { get; set; }
        public int ReportLayout { get; set; }
        public int Copies { get; set; }

        public bool Validate()
        {
            var validator = new FinishOptionsConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class FinishOptionsConfigurationValidator : AbstractValidator<FinishOptionsConfiguration>
        { }
    }
}
