using Ism.Common.Core.Configuration;
using System.Collections.Generic;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class AdministratorGeneralConfiguration
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class InstitutionGeneralConfiguration
    {
        public string Name { get; set; }
        public AdministratorGeneralConfiguration Administrator { get; set; }
    }

    public class GeneralGeneralConfiguration
    {
        public int CacheDuration { get; set; }
        public int SurgeryMode { get; set; }
        public bool AdHocLabelsAllowed { get; set; }
    }

    public class RoutingModeGeneralConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class GeneralConfiguration : IConfigurationPoco
    {
        public InstitutionGeneralConfiguration Institution { get; set; }
        public GeneralGeneralConfiguration General { get; set; }
        public List<RoutingModeGeneralConfiguration> RoutingModes { get; set; }

        public bool Validate()
        {
            return true;
        }
    }
}
