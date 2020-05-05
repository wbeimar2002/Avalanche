using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class ApiSettings
    {
        [JsonProperty("apiUrl")]
        public Uri ApiUrl { get; set; }

        [JsonProperty("secureServerUrl")]
        public Uri SecureServerUrl { get; set; }
    }
}
