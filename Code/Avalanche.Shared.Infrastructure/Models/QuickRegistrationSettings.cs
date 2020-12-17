using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class QuickRegistrationSettings
    {
        public bool IsAllowed { get; set; }
        public string DateFormat { get; set; }
        public bool UseAdministratorAsPhysician { get; set; }
    }
}
