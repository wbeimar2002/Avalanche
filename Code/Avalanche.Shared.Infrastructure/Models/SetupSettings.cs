using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class SetupSettings
    {
        [JsonProperty("Mode")]
        public SetupModes Mode { get; set; }

        [JsonProperty("QuickRegistrationAllowed")]
        public bool QuickRegistrationAllowed { get; set; }

        [JsonProperty("AutoFillPhysician")]
        public bool AutoFillPhysician { get; set; }

        [JsonProperty("QuickRegistrationDateFormat")]
        public string QuickRegistrationDateFormat { get; set; }
    }
}
