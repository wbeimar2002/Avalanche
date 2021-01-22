using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class DynamicPropertyViewModel
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

        public SettingTypes SettingType { get; set; }
        public VisualStyles VisualStyle { get; set; }

        public string SourceKey { get; set; }

        [JsonProperty(nameof(SourceValues))]
        public List<SourceKeyValuePairViewModel> SourceValuesSetter { set { _sourceValues = value; } }

        [JsonIgnore]
        private List<SourceKeyValuePairViewModel> _sourceValues;

        [JsonIgnore]
        public List<SourceKeyValuePairViewModel> SourceValues
        {
            get { return this._sourceValues; }
            set { this._sourceValues = value; }
        }

        [JsonProperty(nameof(Value))]
        public string ValueSetter { set { _value = value; } }

        [JsonIgnore]
        private string _value;

        [JsonIgnore]
        public string Value
        {
            get { return this._value; }
            set { this._value = value; }
        }
    }
}
