using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.ViewModels
{
    public class MedpresenceProvisioningViewModel
    {
        public ProvisioningInputParametersViewModel InputParameters { get; set; }
        public ProvisioiningEnvironmentSettingsViewModel EnvironmentSettings { get; set; }
    }

    public class ProvisioningInputParametersViewModel
    {
        public string Environment { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string CustomerName { get; set; }
        public string Department { get; set; }
        public string Speciality { get; set; }
    }

    public class ProvisioiningEnvironmentSettingsViewModel
    {
        public string ClientId { get; set; }
        public string ApiUrl { get; set; }
        public string IdentityUrl { get; set; }
    }
}

