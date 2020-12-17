using Avalanche.Shared.Domain.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class RoutingSettings
    {
        [JsonProperty("Mode")]
        public RoutingModes Mode { get; set; }
    }
}
