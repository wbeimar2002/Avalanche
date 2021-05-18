using Avalanche.Shared.Domain.Enumerations;
using Ism.Common.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class SetupConfiguration : IConfigurationPoco
    {
        public GeneralSetupConfiguration General { get; set; }
        public RegistrationSetupConfiguration Registration { get; set; }
        public RoutingModes SurgeryMode { get; set; }

        public bool Validate()
        {
            return true;
        }
    }

    public class GeneralSetupConfiguration
    {
        public int Mode { get; set; }
        public int SurgeryMode { get; set; }
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