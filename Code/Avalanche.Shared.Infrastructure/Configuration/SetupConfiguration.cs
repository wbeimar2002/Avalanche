using System.Collections.Generic;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(SetupConfiguration), 1)]
    public class SetupConfiguration : IConfigurationPoco
    {
        public GeneralSetupConfiguration General { get; set; }
        public RegistrationSetupConfiguration Registration { get; set; }
        public List<SearchColumnsConfiguration> SearchColumns { get; set; } = new List<SearchColumnsConfiguration>();
        public List<PatientInfoSetupConfiguration> PatientInfo { get; set; } = new List<PatientInfoSetupConfiguration>();

        public bool Validate()
        {
            // TODO: there are a lot of properties/collections here.  Got to be something to validate...
            return true;
        }
    }

    public class PatientInfoSetupConfiguration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ProcedureInfoField Id { get; set; }
        public string LabelTranslationKey { get; set; }
        public bool Display { get; set; }
        public bool Required { get; set; }
    }

    public class SearchColumnsConfiguration
    {
        public string Id { get; set; }
        public string LabelTranslationKey { get; set; }
        public bool Display { get; set; }
    }

    public class GeneralSetupConfiguration
    {
        public int Mode { get; set; }
    }

    public class QuickSetupConfiguration
    {
        public bool IsAllowed { get; set; }
        public string DateFormat { get; set; } = "yyMMdd_HHmmss";
        public string DefaultUserName { get; set; } = "Administrator";
        public Sexes DefaultSex { get; set; } = Sexes.U;
    }

    public class ManualSetupConfiguration
    {
        // TODO remove PhysicianAsLoggedInUser from Manual Registration settings, since it also applies to Quick Register
        public bool PhysicianAsLoggedInUser { get; set; }
    }

    public class RegistrationSetupConfiguration
    {
        public QuickSetupConfiguration Quick { get; set; }
        public ManualSetupConfiguration Manual { get; set; }
    }
}
