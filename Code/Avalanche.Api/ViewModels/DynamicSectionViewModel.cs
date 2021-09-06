using System.Collections.Generic;
using Newtonsoft.Json;

namespace Avalanche.Api.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DynamicSectionViewModel
    {
        public string? TitleTranslationKey { get; set; }
        public string? Metadata { get; set; }
        public string? JsonKey { get; set; }
        public string? Schema { get; set; }

        public List<DynamicSectionViewModel>? Sections { get; set; }
        public List<DynamicSettingViewModel>? Settings { get; set; }
    }
}
