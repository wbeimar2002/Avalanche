using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class TimeoutSettings
    {
        [JsonProperty("CheckListFileName")]
        public string CheckListFileName { get; set; }
    }
}
