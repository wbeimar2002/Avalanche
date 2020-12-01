using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class SetupSettings
    {
        public SetupModes Mode { get; set; }
        public long CacheDuration { get; set; }
        public bool DepartmentsSupported { get; set; }
        public AdministratorSettings Administrator { get; set; }
        public QuickRegistrationSettings QuickRegistration { get; set; }
        public ManualRegistrationSettings ManualRegistration { get; set; }
    }
}
