using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class SettingViewModel
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

        [JsonProperty(nameof(PoliciesValues))]
        public IList<KeyValuePairViewModel> PoliciesValuesSetter { set { _policiesValues = value; } }

        [JsonIgnore]
        private IList<KeyValuePairViewModel> _policiesValues;

        [JsonIgnore]
        public IList<KeyValuePairViewModel> PoliciesValues
        {
            get { return this._policiesValues; }
            set { this._policiesValues = value; }
        }

        [JsonProperty(nameof(Dependencies))]
        public List<DependencySettingViewModel> DependenciesSetter { set { _dependencies = value; } }

        [JsonIgnore]
        private List<DependencySettingViewModel> _dependencies;

        [JsonIgnore]
        public List<DependencySettingViewModel> Dependencies
        {
            get { return this._dependencies; }
            set { this._dependencies = value; }
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

        [JsonProperty(nameof(ReadOnly))]
        public bool ReadOnlySetter { set { _readOnly = value; } }

        [JsonIgnore]
        private bool _readOnly;

        [JsonIgnore]
        public bool ReadOnly
        {
            get { return this._readOnly; }
            set { this._readOnly = value; }
        }
    }
}
