using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class TimeoutSettings
    {
        [JsonProperty("UseVideo")]
        public bool UseVideo { get; set; }

        [JsonProperty("CheckListFileName")]
        public string CheckListFileName { get; set; }

        [JsonProperty("VideoStreamId")]
        public string VideoStreamId { get; set; }
    }
}
