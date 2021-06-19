using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ContentViewModel
    {
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public int? Length { get; set; }
        public DateTimeOffset CaptureTimeUtc { get; set; }
    }
}
