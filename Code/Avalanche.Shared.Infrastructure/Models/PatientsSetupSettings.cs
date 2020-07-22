using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class PatientsSetupSettings
    {
        [JsonProperty("QuickRegistrationAllowed")]
        public bool QuickRegistrationAllowed { get; set; }
    }
}
