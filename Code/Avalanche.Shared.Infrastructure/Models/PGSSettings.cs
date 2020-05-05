using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class PGSSettings
    {
        [JsonProperty("VideoStreamId")]
        public string VideoStreamId { get; set; }
    }
}
