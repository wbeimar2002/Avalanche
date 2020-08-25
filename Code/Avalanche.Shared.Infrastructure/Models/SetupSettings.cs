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
        public SetupMode Mode { get; set; }
        [JsonProperty("QuickRegistrationAllowed")]
        public bool QuickRegistrationAllowed { get; set; }
    }
}
