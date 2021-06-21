using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ImageContentViewModel
    {
        public string FileName { get; set; }
        public DateTimeOffset CaptureTimeUtc { get; set; }
        public string Thumbnail { get; internal set; }
    }
}
