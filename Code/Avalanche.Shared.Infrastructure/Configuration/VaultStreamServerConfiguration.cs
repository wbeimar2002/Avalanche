using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;
namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(VaultStreamServerConfiguration), 1)]
    public class VaultStreamServerConfiguration : IConfigurationPoco
    {
        public string ServerHost { get; set; }
        public uint ServerPort { get; set; }

        public bool Validate()
        {
            var validator = new VaultStreamServerConfigurationValidator();
            validator.ValidateAndThrow(this);
            return true;
        }

        private sealed class VaultStreamServerConfigurationValidator : AbstractValidator<VaultStreamServerConfiguration>
        { }
    }
}
