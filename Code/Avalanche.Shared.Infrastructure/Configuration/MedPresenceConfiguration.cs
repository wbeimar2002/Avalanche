using FluentValidation;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(MedPresenceConfiguration), 1)]
    public class MedPresenceConfiguration : IConfigurationPoco
    {
        public SessionMedPresenceConfiguration Session { get; set; }

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

    public class SessionMedPresenceConfiguration
    {
        public int Timeout { get; set; }
        public int Duration { get; set; }
        public string RemoteControl { get; set; }

        public ServiceModeConfiguration ServiceMode { get; set; }
        public CollaborationModeConfiguration CollaborationMode { get; set; }
    }

    public class ServiceModeConfiguration
    {
        public string DisplayToShare { get; set; }
        public bool RecordingEnabled { get; set; }
    }
    public class CollaborationModeConfiguration
    {
        public string DisplayToShare { get; set; }
        public bool RecordingEnabled { get; set; }
    }
}
