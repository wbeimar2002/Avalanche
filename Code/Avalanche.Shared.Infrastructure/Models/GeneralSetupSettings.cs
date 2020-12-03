using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class GeneralSetupSettings
    {
        public SetupModes Mode { get; set; }
        public long CacheDuration { get; set; }
        public bool DepartmentsSupported { get; set; }
        public AdministratorSettings Administrator { get; set; }
    }
}
