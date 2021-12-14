using System.Collections.Generic;
using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(ProceduresConfiguration), 1)]
    public class ProceduresConfiguration : IConfigurationPoco
    {
        public bool ProcedureDownloadEnabled { get; set; }
        public bool ShowDefaultSearchResults { get; set; }
        public List<ColumnProceduresSearchConfiguration> Columns { get; set; }

        public bool Validate()
        {
            var validator = new ProceduresConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class ProceduresConfigurationValidator : AbstractValidator<ProceduresConfiguration>
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
