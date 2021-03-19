using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Avalanche.Api.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DynamicSettingViewModel
    {
        public string LabelTranslationKey { get; set; }
        public string DescriptionTranslationKey { get; set; }
        public string Format { get; set; }
        public string PlaceHolderTranslationKey { get; set; }
        public string Icon { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int Steps { get; set; }
        public string DefaultValue { get; set; }
        public bool Required { get; set; }
        public int MaxLength { get; set; }
        public string JsonKey { get; set; }
        public string Policy { get; set; }
        public SettingTypes SettingType { get; set; }
        public VisualStyles VisualStyle { get; set; }
        public string SourceKey { get; set; }
        public VisualStyles SourceVisualStyle { get; set; }
        public DynamicListViewModel CustomList { get; set; }
        public List<ExpandoObject> SourceValues { get; set; }
        public IList<KeyValuePairViewModel> PoliciesValues { get; set; }
        public IList<DynamicDependencySettingViewModel> Dependencies { get; set; }
        public string Value { get; set; }
    }
}
