using System.Collections.Generic;
using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(ProceduresSearchConfiguration), 1)]
    public class ProceduresSearchConfiguration : IConfigurationPoco
    {
        public bool ShowDefaultSearchResults { get; set; }
        public List<ColumnProceduresSearchConfiguration> Columns { get; set; }

        public bool Validate()
        {
            var validator = new ProceduresSearchConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class ProceduresSearchConfigurationValidator : AbstractValidator<ProceduresSearchConfiguration>
        {
        }
    }

    public class ColumnProceduresSearchConfiguration
    {
        public string Id { get; set; }
        public string TranslationKey { get; set; }
        public bool Display { get; set; }
        public bool Sortable { get; set; }
    }
}
