using System.Collections.Generic;
using Ism.Common.Core.Configuration;
using Ism.Common.Core.Configuration.Attributes;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    [ConfigurationRequest(nameof(SetupConfiguration), 1)]
    public class SetupConfiguration : IConfigurationPoco
    {
        public GeneralSetupConfiguration General { get; set; }
        public RegistrationSetupConfiguration Registration { get; set; }
        public List<PatientInfoSetupConfiguration> PatientInfo { get; set; }

        public bool Validate()
        {
            return true;
        }
    }

    public class PatientInfoSetupConfiguration
    {
        public string Id { get; set; }
        public string LabelTranslationKey { get; set; }
        public bool Display { get; set; }
        public bool Required { get; set; }
    }

    public class GeneralSetupConfiguration
    {
        public int Mode { get; set; }
        public int ScreenMode { get; set; }
        public bool DepartmentsSupported { get; set; }
    }

    public class QuickSetupConfiguration
    {
        public bool IsAllowed { get; set; }
        public string DateFormat { get; set; }
        public bool UseAdministratorAsPhysician { get; set; }
    }

    public class ManualSetupConfiguration
    {
        public bool PhysicianAsLoggedInUser { get; set; }
        public bool AutoFillPhysician { get; set; }
    }

    public class RegistrationSetupConfiguration
    {
        public QuickSetupConfiguration Quick { get; set; }
        public ManualSetupConfiguration Manual { get; set; }
    }
}
