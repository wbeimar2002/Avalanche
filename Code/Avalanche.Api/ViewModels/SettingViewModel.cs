using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.ViewModels
{
    public class SettingViewModel
    {
        public string LabelTranslationKey { get; set; }
        public string DescriptionTranslationKey { get; set; }
        public string Format { get; set; }
        public string PlaceHolder { get; set; }
        public string Icon { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int Steps { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public bool ReadOnly { get; set; }
        public bool Required { get; set; }
        public int MaxLength { get; set; }
        public string JsonKey { get; set; }
        public SettingTypes SettingType { get; set; }
        public VisualStyles VisualStyle { get; set; }
        public string SourceKey { get; set; }
        public VisualStyles SourceVisualStyle { get; set; }
        public List<SourceKeyValuePairViewModel> SourceValues { get; set; }
        public List<DependencySettingViewModel> Dependencies { get; set; }
    }
}
