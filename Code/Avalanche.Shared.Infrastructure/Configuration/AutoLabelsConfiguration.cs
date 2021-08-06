using System.Collections.Generic;
using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;


namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(AutoLabelsConfiguration), 1)]
    public class AutoLabelsConfiguration : IConfigurationPoco
    {
        public List<AutoLabelAutoLabelsConfiguration> AutoLabels { get; set; }

        public bool Validate()
        {
            var validator = new AutoLabelsConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class AutoLabelsConfigurationValidator : AbstractValidator<AutoLabelsConfiguration>
        {
        }
    }

    public class AutoLabelAutoLabelsConfiguration
    {
        public int? ProcedureTypeId { get; set; }
        public int LabelId { get; set; }
        public string Color { get; set; }
        public int Index { get; set; }
    }
}
