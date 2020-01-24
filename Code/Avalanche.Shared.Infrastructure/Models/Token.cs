using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
