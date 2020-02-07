using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class ConfigSettings
    {
        [JsonProperty("api_address")]
        public string IpAddress { get; set; }
    }
}
