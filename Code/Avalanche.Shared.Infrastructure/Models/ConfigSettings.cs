using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class ConfigSettings
    {
        [JsonProperty("api")]
        public ApiSettings Api { get; set; }
    }
}
